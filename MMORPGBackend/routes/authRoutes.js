import { User } from "../Models/userModel.js";
import { hashPassword, comparePassword } from "../utils/hash.js";
import jwt from "jsonwebtoken";

export default async function authRoutes(fastify, opts) {
  // Đăng ký
  fastify.post("/register", async (req, reply) => {
    const { username, password, email } = req.body;
    try {
      const exists = await User.findOne({ username });
      if (exists) return reply.code(400).send({ error: "Username đã tồn tại" });

      const password_hash = await hashPassword(password);
      const user = await User.create({ username, password_hash, email });
      reply.send({ message: "Đăng ký thành công", userId: user._id });
    } catch (err) {
      console.error(err);
      reply.code(500).send({ error: "Server error" });
    }
  });

  // Đăng nhập
  fastify.post("/login", async (req, reply) => {
    const { username, password } = req.body;
    try {
      const user = await User.findOne({ username });
      if (!user) return reply.code(400).send({ error: "Sai tài khoản" });

      const match = await comparePassword(password, user.password_hash);
      if (!match) return reply.code(400).send({ error: "Sai mật khẩu" });

      user.last_login = new Date();
      await user.save();

      const token = jwt.sign({ id: user._id }, process.env.JWT_SECRET, {
        expiresIn: "7d"
      });

      reply.send({ message: "Đăng nhập thành công", token });
    } catch (err) {
      console.error(err);
      reply.code(500).send({ error: "Server error" });
    }
  });
}