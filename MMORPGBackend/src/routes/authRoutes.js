import { User } from "../models/userModel.js";
import { hashPassword, comparePassword } from "../../utils/hash.js";

export default async function authRoutes(fastify, opts) {
  // 📘 Schema cho Swagger + Validation (rút gọn)
  const registerSchema = {
    schema: {
      summary: "Đăng ký tài khoản mới (hoặc xác nhận guest bằng gameId)",
      tags: ["Auth"],
      body: {
        type: "object",
        required: ["username", "password", "email"],
        properties: {
          username: { type: "string" },
          password: { type: "string" },
          email: { type: "string" },
          gameId: { type: "string" }, // nếu có -> xác nhận guest và gán username/password/email
        },
      },
      response: {
        200: {
          description: "Kết quả đăng ký",
          type: "object",
          additionalProperties: true,
        },
      },
    },
  };

  const loginSchema = {
    schema: {
      summary: "Đăng nhập (bằng username/password hoặc bằng gameId của guest)",
      tags: ["Auth"],
      body: {
        type: "object",
        properties: {
          username: { type: "string" },
          password: { type: "string" },
          gameId: { type: "string" }, // ưu tiên nếu có
        },
      },
      response: {
        200: { type: "object", additionalProperties: true },
      },
    },
  };

  // Đăng ký hoặc xác nhận guest bằng gameId
  fastify.post("/register", registerSchema, async (req, reply) => {
    const { username, password, email, gameId } = req.body || {};
    try {
      // Nếu có gameId -> tìm guest và "claim" account
      if (gameId) {
        const guest = await User.findOne({ gameId });
        if (!guest) return reply.code(404).send({ error: "Không tìm thấy guest với gameId" });

        // kiểm tra username đã có (ngoại trừ chính guest)
        const exists = await User.findOne({ username });
        if (exists && String(exists._id) !== String(guest._id)) {
          return reply.code(400).send({ error: "Username đã tồn tại" });
        }

        guest.username = username;
        guest.password_hash = await hashPassword(password);
        guest.email = email;
        // khi claim, xóa hoặc giữ gameId tuỳ bạn; ở đây giữ (vẫn có thể dùng)
        await guest.save();

        const resp = guest.toObject();
        delete resp.password_hash;
        return reply.send({ message: "Claim tài khoản guest thành công", user: resp });
      }

      // đăng ký thông thường
      const exists = await User.findOne({ username });
      if (exists) return reply.code(400).send({ error: "Username đã tồn tại" });

      const password_hash = await hashPassword(password);
      const user = await User.create({
        username,
        password_hash,
        email,
        // character sẽ được khởi tạo sau qua /player/init
      });

      const resp = user.toObject();
      delete resp.password_hash;
      return reply.send({ message: "Đăng ký thành công", user: resp });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Đăng nhập: hỗ trợ gameId (guest) hoặc username/password
  fastify.post("/login", loginSchema, async (req, reply) => {
    const { username, password, gameId } = req.body || {};
    try {
      if (gameId) {
        const user = await User.findOne({ gameId }).lean();
        if (!user) return reply.code(404).send({ error: "Không tìm thấy gameId" });
        const { password_hash, ...rest } = user;
        return reply.send({ message: "Đăng nhập bằng gameId thành công", user: rest });
      }

      if (!username || !password) return reply.code(400).send({ error: "Thiếu username hoặc password" });

      const user = await User.findOne({ username });
      if (!user) return reply.code(400).send({ error: "Sai tài khoản" });

      const match = await comparePassword(password, user.password_hash);
      if (!match) return reply.code(400).send({ error: "Sai mật khẩu" });

      await user.save(); // giữ để có thể update nếu cần

      const userObj = user.toObject();
      delete userObj.password_hash;
      return reply.send({ message: "Đăng nhập thành công", user: userObj });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Tạo guest mới -> trả về gameId (chỉ trả gameId + _id, không trả username/email/character/characterTypes)
  fastify.post("/guest", async (req, reply) => {
    try {
      const rand = () => Math.random().toString(36).slice(2, 10);
      const gid = `g_${Date.now().toString(36)}_${rand()}`;

      // lưu minimal dữ liệu, đánh dấu isGuest
      const user = await User.create({
        username: "",      // không lộ username
        password_hash: "", // guest không cần password
        email: "",
        isGuest: true,
        gameId: gid,
        character: { class: "", name: "", level: 0, skills: { skill1:0, skill2:0, skill3:0 } }, // empty state
      });

      // Trả về chỉ gameId và _id — không trả characterTypes/character/username/email
      return reply.send({ message: "Guest created", gameId: gid, id: user._id });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Đổi mật khẩu
  fastify.post("/change-password", {
    schema: {
      summary: "Đổi mật khẩu (bằng username+oldPassword hoặc bằng gameId không cần oldPassword)",
      tags: ["Auth"],
      body: {
        type: "object",
        required: ["newPassword"],
        properties: {
          username: { type: "string" },
          oldPassword: { type: "string" },
          gameId: { type: "string" }, // nếu dùng gameId có thể đổi trực tiếp (claim/guest flow)
          newPassword: { type: "string" },
        },
      },
      response: { 200: { type: "object", additionalProperties: true } },
    },
  }, async (req, reply) => {
    const { username, oldPassword, gameId, newPassword } = req.body || {};
    try {
      if (!newPassword) return reply.code(400).send({ error: "Thiếu newPassword" });

      let user = null;
      if (username) user = await User.findOne({ username });
      if (!user && gameId) user = await User.findOne({ gameId });
      if (!user) return reply.code(404).send({ error: "Không tìm thấy user" });

      // Nếu có username + oldPassword -> verify. Nếu chỉ có gameId -> cho phép đổi (guest flow).
      if (username) {
        if (!oldPassword) return reply.code(400).send({ error: "Thiếu oldPassword" });
        const ok = await comparePassword(oldPassword, user.password_hash);
        if (!ok) return reply.code(400).send({ error: "oldPassword không đúng" });
      }

      user.password_hash = await hashPassword(newPassword);
      await user.save();
      return reply.send({ message: "Đổi mật khẩu thành công" });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Lấy tất cả user (đã loại password_hash)
  fastify.get("/users", {
    schema: {
      summary: "Lấy danh sách tất cả người dùng",
      tags: ["Auth"],
      response: { 200: { type: "array", items: { type: "object", additionalProperties: true } } },
    },
  }, async (req, reply) => {
    try {
      const users = await User.find().lean();
      const sanitized = users.map(u => {
        const { password_hash, ...rest } = u;
        return rest;
      });
      return reply.send(sanitized);
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Xem user theo id
  fastify.get("/users/:id", {
    schema: {
      summary: "Lấy thông tin người dùng theo id",
      tags: ["Auth"],
      params: { type: "object", required: ["id"], properties: { id: { type: "string" } } },
      response: { 200: { type: "object", additionalProperties: true }, 404: { type: "object", additionalProperties: true } },
    },
  }, async (req, reply) => {
    const { id } = req.params;
    try {
      const user = await User.findById(id).lean();
      if (!user) return reply.code(404).send({ error: "Không tìm thấy user" });
      const { password_hash, ...rest } = user;
      return reply.send(rest);
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Xóa user theo id
  fastify.delete("/users/:id", {
    schema: {
      summary: "Xóa tài khoản theo id",
      tags: ["Auth"],
      params: { type: "object", required: ["id"], properties: { id: { type: "string" } } },
      response: { 200: { type: "object", additionalProperties: true }, 404: { type: "object", additionalProperties: true } },
    },
  }, async (req, reply) => {
    const { id } = req.params;
    try {
      const deleted = await User.findByIdAndDelete(id);
      if (!deleted) return reply.code(404).send({ error: "Không tìm thấy user để xóa" });
      return reply.send({ message: "Xóa tài khoản thành công" });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });
}
