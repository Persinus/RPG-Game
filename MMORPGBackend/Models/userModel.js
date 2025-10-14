import mongoose from "mongoose";

const userSchema = new mongoose.Schema({
  username: { type: String, unique: true, required: true },
  password_hash: { type: String, required: true },
  email: { type: String, required: true },
  created_at: { type: Date, default: Date.now },
  last_login: { type: Date },

  // nhân vật — để trống khi đăng ký
  character: { type: Object, default: {} },
  equipment: { type: Object, default: {} },
  inventory: { type: Array, default: [] }
});

export const User = mongoose.model("User", userSchema);