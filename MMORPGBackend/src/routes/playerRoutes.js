import { User } from "../../src/models/userModel.js";

export default async function playerRoutes(fastify, opts) {
  fastify.post("/init", {
    schema: {
      body: {
        type: "object",
        required: ["username"],
        properties: { username: { type: "string" } },
      },
      response: {
        200: {
          type: "object",
          properties: {
            message: { type: "string" },
            character: { type: "object" },
          },
        },
      },
    },
    handler: async (req, reply) => {
      const { username } = req.body;
      try {
        const user = await User.findOne({ username });
        if (!user) return reply.code(404).send({ error: "Không tìm thấy người chơi" });

        user.character = { level: 1, hp: 100, gold: 0 };
        await user.save();

        reply.send({ message: "Tạo nhân vật thành công", character: user.character });
      } catch (err) {
        console.error(err);
        reply.code(500).send({ error: "Server error" });
      }
    },
  });
}
