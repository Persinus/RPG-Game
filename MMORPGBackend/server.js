import Fastify from "fastify";
import cors from "fastify-cors";
import dotenv from "dotenv";
import { connectDB } from "./src/db.js";
import authRoutes from "./src/routes/authRoutes.js";

dotenv.config();
const fastify = Fastify({ logger: true });

// Middleware
fastify.register(cors, { origin: "*" });

// Routes
fastify.register(authRoutes, { prefix: "/auth" });

// Start server
const start = async () => {
  try {
    await connectDB();
    await fastify.listen({ port: process.env.PORT, host: "0.0.0.0" });
    console.log("🚀 Server running on port", process.env.PORT);
  } catch (err) {
    fastify.log.error(err);
    process.exit(1);
  }
};