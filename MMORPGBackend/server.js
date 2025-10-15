import Fastify from "fastify";
import cors from "@fastify/cors";
import dotenv from "dotenv";
import { connectDB } from "./src/db.js";
import authRoutes from "./src/routes/authRoutes.js";
import playerRoutes from "./src/routes/playerRoutes.js";
import swagger from "@fastify/swagger";
import swaggerUI from "@fastify/swagger-ui";

dotenv.config();
const fastify = Fastify({ logger: true });

// Middleware
await fastify.register(cors, { origin: "*" });
fastify.addContentTypeParser("application/json", { parseAs: "string" }, function (req, body, done) {
  try {
    const json = JSON.parse(body);
    done(null, json);
  } catch (err) {
    done(err, undefined);
  }
});

// Swagger setup
await fastify.register(swagger, {
  swagger: {
    info: {
      title: "RPG API",
      description: "Fastify API cho game RPG Online",
      version: "1.0.0",
    },
    consumes: ["application/json"],
    produces: ["application/json"],
  },
});

await fastify.register(swaggerUI, {
  routePrefix: "/docs",
  uiConfig: {
    docExpansion: "list",
    deepLinking: false,
  },
});

// Routes
fastify.register(authRoutes, { prefix: "/auth" });
fastify.register(playerRoutes, { prefix: "/player" });

// Route gốc
fastify.get("/", async () => {
  return { message: "🧙‍♂️ RPG API đang chạy! Xem tài liệu tại /docs" };
});

// Start server
const start = async () => {
  try {
    await connectDB();
    await fastify.listen({ port: process.env.PORT || 8080, host: "0.0.0.0" });
    console.log("🚀 Server running on port", process.env.PORT || 8080);
  } catch (err) {
    fastify.log.error(err);
    process.exit(1);
  }
};
start();