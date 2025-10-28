import mongoose from "mongoose";

const petSchema = new mongoose.Schema({
  name: { type: String, default: "" },
  type: { type: String, default: "" }, // ví dụ: "dragon", "wolf"
  level: { type: Number, default: 1 },
  rarity: { type: String, default: "common" }, // common/rare/epic...
  stats: {
    hp: { type: Number, default: 0 },
    attack: { type: Number, default: 0 },
    mana: { type: Number, default: 0 },
  },
  created_at: { type: Date, default: Date.now },
});

const userSchema = new mongoose.Schema({
  username: { type: String, index: true, unique: true, default: "" },
  password_hash: { type: String, default: "" },
  email: { type: String, unique: true, sparse: true, default: "" },
  gameId: { type: String, default: "" },

  // currency
  gold: { type: Number, default: 0 },
  diamonds: { type: Number, default: 0 }, // new currency

  character: {
    class: { type: String, enum: ["archer", "assassin", "saber", ""], default: "" },
    name: { type: String, default: "" },
    level: { type: Number, default: 0 },
    hp: { type: Number },
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

  // pets owned by user
  pets: { type: [petSchema], default: [] },

  // equipment: store equipped pet id (ObjectId referencing subdoc _id)
  equipment: {
    petId: { type: mongoose.Schema.Types.ObjectId, default: null },
    // ... you can keep other slots here
  },

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
