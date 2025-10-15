import { User } from "../models/userModel.js";
import { hashPassword, comparePassword } from "../../utils/hash.js";

export default async function authRoutes(fastify, opts) {
  // 📘 Schema cho Swagger + Validation
  const registerSchema = {
    schema: {
      summary: "Đăng ký tài khoản mới",
      tags: ["Auth"],
      body: {
        type: "object",
        required: ["username", "password", "email"],
        properties: {
          username: { type: "string"  },
          password: { type: "string"},
          email: { type: "string"},
        },
      },
      response: {
        200: {
          description: "Kết quả đăng ký",
          type: "object",
          properties: {
            message: { type: "string" },
            userId: { type: "string" },
          },
        },
      },
    },
  };

  const loginSchema = {
    schema: {
      summary: "Đăng nhập",
      tags: ["Auth"],
      body: {
        type: "object",
        required: ["username", "password"],
        properties: {
          username: { type: "string" },
          password: { type: "string"},
        },
      },
      response: {
        200: {
          description: "Kết quả đăng nhập",
          type: "object",
          properties: {
            message: { type: "string" },
            user: { type: "object" },
          },
        },
      },
    },
  };

  // 🧩 Đăng ký
  fastify.post("/register", registerSchema, async (req, reply) => {
    const { username, password, email } = req.body || {};
    try {
      const exists = await User.findOne({ username });
      if (exists) return reply.code(400).send({ error: "Username đã tồn tại" });

      const password_hash = await hashPassword(password);
      const user = await User.create({
        username,
        password_hash,
        email,
        character: { level: 1, hp: 100, gold: 0 },
      });

      return reply.send({ message: "Đăng ký thành công", userId: user._id });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // 🧩 Đăng nhập
  fastify.post("/login", loginSchema, async (req, reply) => {
    const { username, password } = req.body || {};
    try {
      const user = await User.findOne({ username });
      if (!user) return reply.code(400).send({ error: "Sai tài khoản" });

      const match = await comparePassword(password, user.password_hash);
      if (!match) return reply.code(400).send({ error: "Sai mật khẩu" });

      user.last_login = new Date();
      await user.save();

      // loại bỏ password_hash trước khi trả về
      const userObj = user.toObject();
      delete userObj.password_hash;

      return reply.send({ message: "Đăng nhập thành công", user: userObj });
    } catch (err) {
      console.error(err);
      return reply.code(500).send({ error: "Server error" });
    }
  });

  // ---------------------------------------------------------------------------
  // NEW: Xem toàn bộ tài khoản
  // ---------------------------------------------------------------------------
  fastify.get("/users", {
    schema: {
      summary: "Lấy danh sách tất cả người dùng",
      tags: ["Auth"],
      response: {
        200: {
          type: "array",
          items: { type: "object" },
        },
      },
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

  // ---------------------------------------------------------------------------
  // NEW: Xem user theo id
  // ---------------------------------------------------------------------------
  fastify.get("/users/:id", {
    schema: {
      summary: "Lấy thông tin người dùng theo id",
      tags: ["Auth"],
      params: {
        type: "object",
        required: ["id"],
        properties: { id: { type: "string" } },
      },
      response: {
        200: { type: "object" },
        404: { type: "object" },
      },
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

  // ---------------------------------------------------------------------------
  // NEW: Xóa tài khoản theo id
  // ---------------------------------------------------------------------------
  fastify.delete("/users/:id", {
    schema: {
      summary: "Xóa tài khoản theo id",
      tags: ["Auth"],
      params: {
        type: "object",
        required: ["id"],
        properties: { id: { type: "string" } },
      },
      response: {
        200: {
          type: "object",
          properties: { message: { type: "string" } },
        },
        404: { type: "object" },
      },
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
  fastify.get("/test-users", async (req, reply) => {
  const users = await User.find();
  console.log("Users found:", users.length);
  return users;
});
}
