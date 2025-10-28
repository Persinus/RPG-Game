import { User } from "../models/userModel.js";
import { hashPassword, comparePassword } from "../../utils/hash.js";

export default async function authRoutes(fastify, opts) {
  // Register (no guest/claim)
  const registerSchema = {
    schema: {
      summary: "Đăng ký tài khoản mới",
      tags: ["Auth"],
      body: {
        type: "object",
        required: ["username", "password", "email"],
        properties: {
          username: { type: "string" },
          password: { type: "string" },
          email: { type: "string" },
        },
      },
      response: {
        200: { description: "Kết quả đăng ký", type: "object", additionalProperties: true },
      },
    },
  };

  // Login only by username/password
  const loginSchema = {
    schema: {
      summary: "Đăng nhập (chỉ username + password)",
      tags: ["Auth"],
      body: {
        type: "object",
        required: ["username", "password"],
        properties: {
          username: { type: "string" },
          password: { type: "string" },
        },
      },
      response: { 200: { type: "object", additionalProperties: true } },
    },
  };

  // Change password: require username + oldPassword
  const changePasswordSchema = {
    schema: {
      summary: "Đổi mật khẩu (username + oldPassword required)",
      tags: ["Auth"],
      body: {
        type: "object",
        required: ["username", "oldPassword", "newPassword"],
        properties: {
          username: { type: "string" },
          oldPassword: { type: "string" },
          newPassword: { type: "string" },
        },
      },
      response: { 200: { type: "object", additionalProperties: true } },
    },
  };

  // Register
  fastify.post("/register", registerSchema, async (req, reply) => {
    const { username, password, email } = req.body || {};
    try {
      if (!username || !password || !email) return reply.code(400).send({ error: "Thiếu username/password/email" });

      // check existing username/email
      const byUsername = await User.findOne({ username });
      if (byUsername) return reply.code(400).send({ error: "Username đã tồn tại" });

      const byEmail = await User.findOne({ email });
      if (byEmail) return reply.code(400).send({ error: "Email đã được sử dụng" });

      const password_hash = await hashPassword(password);
      const user = await User.create({ username, password_hash, email });

      const resp = user.toObject();
      delete resp.password_hash;
      return reply.send({ message: "Đăng ký thành công", user: resp });
    } catch (err) {
      // handle duplicate key (race condition)
      if (err && err.code === 11000) {
        return reply.code(400).send({ error: "Username hoặc email đã tồn tại" });
      }
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Login
  fastify.post("/login", loginSchema, async (req, reply) => {
    const { username, password } = req.body || {};
    try {
      if (!username || !password) return reply.code(400).send({ error: "Thiếu username hoặc password" });

      const user = await User.findOne({ username });
      if (!user) return reply.code(400).send({ error: "Sai tài khoản" });

      const match = await comparePassword(password, user.password_hash);
      if (!match) return reply.code(400).send({ error: "Sai mật khẩu" });

      const userObj = user.toObject();
      delete userObj.password_hash;
      return reply.send({ message: "Đăng nhập thành công", user: userObj });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Remove guest endpoint (no guest flow)

  // Change password (username + oldPassword)
  fastify.post("/change-password", changePasswordSchema, async (req, reply) => {
    const { username, oldPassword, newPassword } = req.body || {};
    try {
      const user = await User.findOne({ username });
      if (!user) return reply.code(404).send({ error: "Không tìm thấy user" });

      const ok = await comparePassword(oldPassword, user.password_hash);
      if (!ok) return reply.code(400).send({ error: "oldPassword không đúng" });

      user.password_hash = await hashPassword(newPassword);
      await user.save();
      return reply.send({ message: "Đổi mật khẩu thành công" });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // Keep user listing / detail / delete (unchanged)
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
