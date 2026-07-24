# VocabVault 🗝️

VocabVault is a dynamic, full-stack clone of the popular word-puzzle game **"Words of Wonders" (WoW)**. Players interact with a circular wheel of scrambled letters, dragging connections between them to form words and solve crossword puzzles. The game tracks scores, offers hints, and provides real-time dictionary definitions to help players learn new words.

---

## 🌟 Features

*   **Dynamic Level Generation:** An algorithmic crossword compiler that arranges words dynamically based on base dictionary words.
*   **Progressive Difficulty:** Letters on the circular wheel scale from 5 to 7 characters as you advance through levels.
*   **Dictionary Integration:** Real-time word definition popups sourced from a public dictionary API.
*   **Database Caching:** Meanings are cached locally in MS SQL Server to minimize third-party API calls and optimize performance.
*   **Interactive Letter Circle:** Responsive SVG/Canvas drawing interface supporting mouse drag and mobile touch gestures.
*   **Smart Hints:** Uncover mysterious letters on the grid for 50 score points.

---

## 🛠️ Technology Stack

### Backend
*   **Runtime:** .NET Core (9.0+)
*   **Framework:** ASP.NET Core Web API
*   **ORM:** Entity Framework Core
*   **Database:** Microsoft SQL Server
*   **External API:** [Free Dictionary API](https://dictionaryapi.dev/)

### Frontend
*   **Framework:** Angular (Standalone architecture)
*   **State Management:** Angular Signals
*   **Graphics & Interaction:** Inline SVG with event-driven collision mapping for drawing lines.
*   **Styling:** Vanilla CSS (Curated dark mode theme with glassmorphism)

---

## 📂 Project Structure

```text
VocabVault/
├── backend/
│   ├── WoWGame.slnx                # .NET Solution file
│   └── WoWGame.Api/
│       ├── Controllers/            # API endpoints (Levels, Meanings, Players)
│       ├── Data/                   # DbContext and DB Entities
│       ├── Services/               # Crossword algorithm, dictionary & level services
│       ├── Resources/              # dictionary.txt source word list
│       └── Program.cs              # Server configuration and entry point
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/         # GameBoard, LetterCircle, WordGrid, MeaningPopup
│   │   │   ├── services/           # GameService for HTTP calls
│   │   │   └── models/             # TypeScript models
│   │   └── index.html              # Main HTML entry file
│   ├── angular.json                # Angular configuration
│   └── package.json                # Frontend dependencies and scripts
└── README.md                       # This file
```

---

## 🚀 Running the App Locally

### 1. Database Setup
Ensure you have **Microsoft SQL Server** installed and running locally, or host it inside Docker.
Update the connection string in `backend/WoWGame.Api/appsettings.json` under `ConnectionStrings:DefaultConnection`.

### 2. Run the Backend API
1. Navigate to the API folder:
   ```bash
   cd backend/WoWGame.Api
   ```
2. Restore dependencies and run the server:
   ```bash
   dotnet run
   ```
The API will start running at `http://localhost:5194` (or `https://localhost:7194`), and Entity Framework will automatically generate the database and run migrations.

### 3. Run the Frontend
1. Navigate to the frontend folder:
   ```bash
   cd frontend
   ```
2. Install npm packages:
   ```bash
   npm install
   ```
3. Start the Angular development server:
   ```bash
   npm start
   ```
4. Open your browser and navigate to `http://localhost:4200`.

---

## ⚡ Deployment Summary

*   **Database:** Deploy to Azure SQL Database or AWS RDS.
*   **API Service:** Deploy to Azure App Service or AWS Elastic Beanstalk. Ensure `ConnectionStrings__DefaultConnection` is set in the environment variables.
*   **Frontend Static Site:** Build using `npm run build` and deploy the output static files inside `frontend/dist/frontend/browser/` to Azure Static Web Apps, Netlify, Vercel, or AWS S3 + CloudFront.
