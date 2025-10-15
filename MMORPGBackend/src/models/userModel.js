import mongoose from "mongoose";

const userSchema = new mongoose.Schema({
  username: { type: String, unique: true, required: true },
  password_hash: { type: String, required: true },
  email: { type: String, required: true },
  created_at: { type: Date, default: Date.now },
  last_login: { type: Date },
  character: {
    name: { type: String, default: "" },
    level: { type: Number, default: 1 },
    hp: { type: Number, default: 100 },
    gold: { type: Number, default: 0 },
    exp: { type: Number, default: 0 },
  },
  equipment: { type: Object, default: {} },
  inventory: { type: Array, default: [] },
});

export const User = mongoose.model("User", userSchema);
