export interface GridWordPlacement {
  word: string;
  startX: number;
  startY: number;
  direction: 'H' | 'V';
}

export interface Level {
  levelNumber: number;
  letters: string;
  words: string[];
  grid: GridWordPlacement[];
  width: number;
  height: number;
}

export interface WordMeaning {
  word: string;
  meaning: string;
}

export interface Player {
  username: string;
  currentLevel: number;
  score: number;
}
