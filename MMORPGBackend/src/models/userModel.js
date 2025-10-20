import mongoose from "mongoose";

const userSchema = new mongoose.Schema({
  username: { type: String, index: true, default: "" },
  password_hash: { type: String, default: "" },
  email: { type: String, default: "" },
  isGuest: { type: Boolean, default: false },
  created_at: { type: Date, default: Date.now },
  gameId: { type: String, default: "" }, // GAMEId để đăng nhập dưới dạng Guest

  // Không lưu characterTypes vào mỗi user. Templates nằm ở src/config/characterTypes.js

  // Thông tin nhân vật lưu trạng thái hiện tại (mặc định: chưa chọn)
  character: {
    class: { type: String, enum: ["archer", "assassin", "saber", ""], default: "" },
    name: { type: String, default: "" },
    level: { type: Number, default: 0 }, // 0 = chưa khởi tạo / chưa chọn class
    hp: { type: Number },      // sẽ được gán từ template khi chọn class
    attack: { type: Number },
    mana: { type: Number },
    gold: { type: Number, default: 0 },
    exp: { type: Number, default: 0 },
    skills: {
      skill1: { type: Number, default: 0 },
      skill2: { type: Number, default: 0 },
      skill3: { type: Number, default: 0 },
    },
  },

  equipment: { type: Object, default: {} },
  inventory: { type: Array, default: [] },
});

// sanitize output
userSchema.set("toJSON", {
  transform(doc, ret) {
    delete ret.password_hash;
    return ret;
  },
});
userSchema.set("toObject", {
  transform(doc, ret) {
    delete ret.password_hash;
    return ret;
  },
});

export const User = mongoose.model("User", userSchema);
