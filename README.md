# VocabVault 🗝️

### 🎮 **Live Demo:** [https://vocab-vault.vercel.app](https://vocab-vault.vercel.app)
### ⚙️ **Live API:** [https://vocabvault-api.onrender.com](https://vocabvault-api.onrender.com)

VocabVault is a dynamic, premium, full-stack crossword connect word puzzle game inspired by **"Words of Wonders" (WoW)**. Players connect letters arranged in a circle using click-and-drag or touch paths to form words and fill them into an intersecting crossword grid.

This project is built using a modern **Angular 21** frontend, an **ASP.NET Core Web API (.NET 10)** backend, and a serverless **PostgreSQL** database. It is optimized to be deployed completely for free in the cloud.

---

## 🌟 Key Features

*   **Algorithmic Crossword Generation**: An API-driven layout solver that dynamically places intersecting words onto a compact grid based on dictionary constraints.
*   **Progressive Level Scaling**: Game complexity dynamically increases as you level up, automatically scaling the letter wheel size (from 5 up to 7 characters).
*   **Word Meaning Lookup & Caching**: Integrates with a dictionary API to fetch word definitions on correct guesses, caching them locally in PostgreSQL to minimize API limits.
*   **Tactile Circle Connectivity**: Responsive SVG connect wheel supporting path-tracing mouse drags and mobile portrait touch events.
*   **Smooth Animations & FX**:
    *   **3D Card Flip**: Grid cells execute a 3D Y-axis spin when solved or revealed.
    *   **Tactile Letter Pulse**: Connected circular nodes bounce in size during paths.
    *   **Score Tally Count-up**: Scores increment smoothly over 600ms using a quadratic ease-out interpolation curve.
    *   **Particle Confetti Explosion**: Solving words and clearing levels triggers colored physics-based sparklers that float, rotate, and fade with gravity.
*   **Modern Styling**: Styled using **Bootstrap 5**, **Bootstrap Icons**, and custom vanilla CSS for a sleek dark mode glassmorphism theme.
*   **Persistent Progress**: Player stats (score and level index) are synced in real-time to the database, resuming exactly where you left off.

---

## 🛠️ Technology Stack

| Layer | Technologies Used |
| :--- | :--- |
| **Frontend** | Angular 21, Angular Signals, Bootstrap 5, Bootstrap Icons, SVG Vector Paths |
| **Backend API** | .NET 10 Web API, Entity Framework Core (EF Core) |
| **Database** | PostgreSQL (Neon.tech or Supabase serverless) |
| **Hosting (Free)** | Vercel (Frontend), Render.com Docker Container (Backend) |

---

## 📂 Project Structure

```text
VocabVault/
├── backend/
│   ├── WoWGame.slnx                # Visual Studio Solution file
│   └── WoWGame.Api/
│       ├── Controllers/            # API Endpoints (Levels, Meanings, Players)
│       ├── Data/                   # EF Core DbContext, Entities, and Migrations
│       ├── Services/               # Crossword compilation, level loader, dictionary cache
│       ├── Resources/              # Source dictionary word list (dictionary.txt)
│       ├── Program.cs              # API entry point & CORS configuration
│       └── appsettings.json        # Database connection configurations
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/         # GameBoard, LetterCircle, WordGrid, MeaningPopup
│   │   │   ├── services/           # GameService for HTTP backend communication
│   │   │   └── models/             # TypeScript models
│   │   ├── index.html              # Main HTML page template
│   │   └── styles.css              # Reset rules and global Bootstrap imports
│   ├── angular.json                # Angular compiler settings
│   └── package.json                # Frontend dependencies (Bootstrap, etc.)
├── Dockerfile                      # Multi-stage build script for Render backend
├── .dockerignore                   # Exclude list for Docker build context
└── README.md                       # This documentation file
```

---

## 🚀 Local Development Setup

To run the application locally on your machine:

### 1. Database Connection
Update the connection string in [appsettings.json](file:///E:/WoWGame/backend/WoWGame.Api/appsettings.json) under `ConnectionStrings:DefaultConnection`. You can use a local PostgreSQL server or paste your free Neon.tech PostgreSQL connection string (using standard key-value format):
```json
"DefaultConnection": "Host=your-host;Database=your-db;Username=your-user;Password=your-pass;Port=5432;SSL Mode=Require;Trust Server Certificate=true;"
```

### 2. Run the Backend API
1. Navigate to the API folder:
   ```bash
   cd backend/WoWGame.Api
   ```
2. Run the server:
   ```bash
   dotnet run
   ```
The backend will compile and start listening on `http://localhost:5194`. The tables are automatically created in the database on startup via `context.Database.EnsureCreated()`.

### 3. Run the Frontend
1. Navigate to the frontend folder:
   ```bash
   cd frontend
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the dev server:
   ```bash
   npm start
   ```
4. Open your browser to `http://localhost:4200` to play!

---

## ☁️ 100% Free Production Deployment

Follow this guide to host the entire stack for free:

### 1. Database (Neon.tech or Supabase)
1. Register a free PostgreSQL database at **[Neon.tech](https://neon.tech/)**.
2. Format your connection URI into a key-value connection string (e.g. `Host=...;Database=...;Username=...;Password=...;Port=5432;SSL Mode=Require;Trust Server Certificate=true;`).

### 2. Backend API (Render.com)
1. Create a free account at **[Render.com](https://render.com/)**.
2. Select **New +** $\rightarrow$ **Web Service** and link your GitHub repository.
3. Configure the settings:
   - **Runtime**: Select **Docker** (Render will automatically detect the root `Dockerfile`).
   - **Instance Type**: Select **Free**.
4. Under **Environment Variables**, add the connection string:
   - Key: `ConnectionStrings__DefaultConnection`
   - Value: *(Your converted Key-Value connection string)*
5. Click **Deploy**. Render will host your API at a public link (e.g. `https://vocabvault-api.onrender.com`).
   > *Note: Render free services go to sleep after 15 minutes of inactivity. When woken up, the first request can take ~50 seconds to complete.*

### 3. Frontend App (Vercel)
1. Open [game.service.ts](file:///E:/WoWGame/frontend/src/app/services/game.service.ts) and set the API url to your Render backend:
   ```typescript
   private readonly apiUrl = 'https://your-render-name.onrender.com/api';
   ```
2. Push your changes to GitHub.
3. Sign up at **[Vercel](https://vercel.com/)** and import your repository.
4. Set the following parameters:
   - **Framework Preset**: Angular.
   - **Root Directory**: `frontend`.
   - **Build and Output Settings** (Override):
     - **Build Command**: `npm run build`
     - **Output Directory**: `dist/frontend/browser`
5. Click **Deploy**. Vercel will host your site on a free public domain (e.g., `https://vocabvault.vercel.app`).
