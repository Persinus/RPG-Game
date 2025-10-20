import { User } from "../models/userModel.js";
import { characterTypes } from "../config/characterTypes.js";

export default async function playerRoutes(fastify, opts) {
  // /init: giữ backward-compat nhưng không ép set class/level nếu chưa chọn
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

        // nếu user đã chọn class, giữ; nếu chưa thì chỉ đảm bảo character object tồn tại (level 0)
        if (!user.character) {
          user.character = { class: "", name: "", level: 0, skills: { skill1:0, skill2:0, skill3:0 } };
          await user.save();
        }

        return reply.send({ message: "Init done", character: user.character });
      } catch (err) {
        console.error(err);
        reply.code(500).send({ error: "Server error" });
      }
    },
  });

  // select-class: dùng template từ config, gán đầy đủ state và trả về user (không có password_hash)
  fastify.post("/select-class", {
    schema: {
      summary: "Chọn class theo gameId và trả về full thông tin nhân vật",
      tags: ["Player"],
      body: {
        type: "object",
        required: ["gameId", "class"],
        properties: {
          gameId: { type: "string" }, // id riêng của player (guest hoặc đã đăng ký)
          class: { type: "string", enum: ["archer", "assassin", "saber"] },
          name: { type: "string" },
        },
      },
      response: {
        200: { type: "object", additionalProperties: true },
        400: { type: "object", additionalProperties: true },
        404: { type: "object", additionalProperties: true },
      },
    },
    handler: async (req, reply) => {
      const { gameId, class: chosenClass, name = "" } = req.body;
      try {
        const user = await User.findOne({ gameId });
        if (!user) return reply.code(404).send({ error: "Không tìm thấy player với gameId này" });

        const ct = characterTypes[chosenClass];
        if (!ct) return reply.code(400).send({ error: "Class không hợp lệ" });

        user.character = {
          class: chosenClass,
          name: name || user.character?.name || "",
          level: 1,
          hp: ct.hp,
          attack: ct.attack,
          mana: ct.mana,
          gold: 0,
          exp: 0,
          skills: { skill1: 1, skill2: 1, skill3: 1 },
        };

        await user.save();
        const userObj = user.toObject();
        delete userObj.password_hash;
        return reply.send({ message: "Chọn class thành công", user: userObj });
      } catch (err) {
        console.error(err);
        reply.code(500).send({ error: "Server error" });
      }
    },
  });
}
