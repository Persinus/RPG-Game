export const characterTypes = {
  archer: {
    hp: 90,
    attack: 60,
    mana: 140, // tổng mana stat của class (giữ nếu cần dùng cho bar/tối đa)
    skills: {
      skill1: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
      skill2: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
      skill3: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
    },
  },

  assassin: {
    hp: 70,
    attack: 150,
    mana: 60,
    skills: {
      skill1: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
      skill2: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
      skill3: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
    },
  },

  saber: {
    hp: 160,
    attack: 80,
    mana: 80,
    skills: {
      skill1: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
      skill2: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
      skill3: [{ level: 1 }, { level: 2 }, { level: 3 }, { level: 4 }, { level: 5 }, { level: 6 }, { level: 7 }],
    },
  },
};