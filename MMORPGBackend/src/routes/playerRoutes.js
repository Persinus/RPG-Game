import { User } from "../models/userModel.js";
import { characterTypes } from "../config/characterTypes.js";

export default async function playerRoutes(fastify, opts) {
  // Init (unchanged)
  fastify.post("/init", {
    schema: {
      summary: "Khởi tạo trạng thái character nếu chưa có (không ép chọn class)",
      tags: ["Player"],
      body: {
        type: "object",
        required: ["username"],
        properties: { username: { type: "string" } },
      },
      response: { 200: { type: "object", additionalProperties: true } },
    },
    handler: async (req, reply) => {
      const { username } = req.body;
      try {
        const user = await User.findOne({ username });
        if (!user) return reply.code(404).send({ error: "Không tìm thấy người chơi" });

        if (!user.character) {
          user.character = { class: "", name: "", level: 0, hp: null, attack: null, mana: null, gold: 0, exp: 0, skills: { skill1:0, skill2:0, skill3:0 } };
          await user.save();
        }

        return reply.send({ message: "Init done", character: user.character });
      } catch (err) {
        console.error(err);
        reply.code(500).send({ error: "Server error" });
      }
    },
  });

  // select-class: use path parameter :gameId (Parameters) and body for class/name
  fastify.post("/select-class/:gameId", {
    schema: {
      summary: "Chọn class theo gameId (gameId là parameter trong URL). Lưu trong DB chỉ là skill levels (skill1/2/3: số). Các hệ số/ % (multiplier) tính trên client/Unity.",
      tags: ["Player"],
      params: {
        type: "object",
        required: ["gameId"],
        properties: { gameId: { type: "string", description: "gameId của player" } },
      },
      body: {
        type: "object",
        required: ["class"],
        properties: {
          class: { type: "string", enum: ["archer", "assassin", "saber"], description: "Lựa chọn class" },
          name: { type: "string", description: "Tên nhân vật (tuỳ chọn)" },
        }
      },
      response: {
        200: {
          type: "object",
          additionalProperties: true,
          description: "Trả về user (không có password_hash). character.skills chỉ chứa levels (số)."
        },
        400: { type: "object", additionalProperties: true },
        404: { type: "object", additionalProperties: true },
      },
    },
    handler: async (req, reply) => {
      const { gameId } = req.params;
      const { class: chosenClass, name = "" } = req.body;
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

  // Optional: select by username (registered users) - kept if desired
  fastify.post("/select-class-by-username", {
    schema: {
      summary: "Chọn class theo username (dành cho user đã đăng ký)",
      tags: ["Player"],
      body: {
        type: "object",
        required: ["username", "class"],
        properties: {
          username: { type: "string" },
          class: { type: "string", enum: ["archer", "assassin", "saber"] },
          name: { type: "string" },
        },
      },
      response: { 200: { type: "object", additionalProperties: true } },
    },
    handler: async (req, reply) => {
      const { username, class: chosenClass, name = "" } = req.body;
      try {
        const user = await User.findOne({ username });
        if (!user) return reply.code(404).send({ error: "Không tìm thấy user" });

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
