# 🎮 VocabVault 🗝️

**VocabVault** is a dynamic, full-stack word puzzle game inspired by *Words of Wonders (WoW)*.
Players interact with a circular wheel of scrambled letters, drawing connections to form words and solve crossword puzzles.

The game blends **fun gameplay with learning**, offering real-time word meanings, scoring, hints, and progressive difficulty.

---

# 🌟 Features

* 🧩 **Dynamic Level Generation**
  Algorithmically generates crossword puzzles from base dictionary words.

* 📈 **Progressive Difficulty**
  Letter count increases (5 → 7 letters) as levels advance.

* 📖 **Dictionary Integration**
  Real-time word meanings fetched from a public dictionary API.

* 🗄️ **Database Caching**
  Word meanings are stored locally in SQL Server to reduce API calls and improve performance.

* 🎯 **Interactive Letter Circle**
  Drag-to-connect interface using SVG with support for mouse and touch gestures.

* 💡 **Smart Hint System**
  Reveal hidden letters using score points.

---

# 🛠️ Technology Stack

## ⚙️ Backend

* **Runtime:** .NET Core (9.0+)
* **Framework:** ASP.NET Core Web API
* **ORM:** Entity Framework Core
* **Database:** Microsoft SQL Server
* **External API:** Free Dictionary API

## 🌐 Frontend

* **Framework:** Angular (Standalone Architecture)
* **State Management:** Angular Signals
* **Graphics & Interaction:** Inline SVG with event-driven drag handling
* **Styling:** Vanilla CSS (Dark mode + glassmorphism UI)

---

# 📂 Project Structure

```id="n4k1pu"
VocabVault/
├── backend/
│   ├── WoWGame.slnx
│   └── WoWGame.Api/
│       ├── Controllers/        # API endpoints (Levels, Meanings, Players)
│       ├── Data/               # DbContext and Entities
│       ├── Services/           # Game logic, crossword generator
│       ├── Resources/          # dictionary.txt word list
│       └── Program.cs          # App entry point
│
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/     # GameBoard, LetterCircle, WordGrid, MeaningPopup
│   │   │   ├── services/       # API communication
│   │   │   └── models/         # TypeScript models
│   │   └── index.html
│   ├── angular.json
│   └── package.json
│
└── README.md
```

---

# 🚀 Running the App Locally

## 1️⃣ Database Setup

* Install **Microsoft SQL Server** (or use Docker)
* Update connection string in:

```id="p4bq0d"
backend/WoWGame.Api/appsettings.json
```

```json id="zslp9l"
"ConnectionStrings": {
  "DefaultConnection": "YOUR_CONNECTION_STRING"
}
```

---

## 2️⃣ Run Backend API

```bash id="l1i2tm"
cd backend/WoWGame.Api
dotnet restore
dotnet run
```

* API runs at:

  * http://localhost:5194
  * https://localhost:7194

* Entity Framework will:

  * Create database
  * Apply migrations automatically

---

## 3️⃣ Run Frontend

```bash id="hjg2i9"
cd frontend
npm install
npm start
```

Open browser:

```id="5txd42"
http://localhost:4200
```

---

# 🔄 How the Game Works

1. Player opens the app
2. Backend loads or generates a level
3. Letters appear in circular layout
4. Player forms words by dragging across letters
5. Valid words:

   * Increase score
   * Show meaning popup
6. Complete all words → unlock next level

---

# 🎯 Core Highlights

* ⚡ Smart crossword generation algorithm
* 🧠 Educational gameplay with vocabulary learning
* 🚀 Optimized with DB caching
* 📱 Fully responsive & touch-friendly

---

# 🔮 Future Enhancements

* 🏆 Leaderboards
* 📅 Daily challenges
* 🌍 Multi-language support
* 📱 Mobile app version
* 🎵 Sound effects & animations

---

# 🤝 Contributing

Contributions are welcome!
Feel free to fork this repository and submit a pull request.

---

# 📜 License

This project is for educational and portfolio purposes.

---

# 💡 Author

Built with ❤️ using Angular + .NET
