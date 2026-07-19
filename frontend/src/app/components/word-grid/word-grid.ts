import { Component, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Level } from '../../models/game.models';

interface GridCell {
  char: string;
  isUsed: boolean;
  words: string[];
  x: number;
  y: number;
}

@Component({
  selector: 'app-word-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './word-grid.html',
  styleUrl: './word-grid.css'
})
export class WordGrid {
  level = input.required<Level>();
  solvedWords = input.required<string[]>();
  revealedCoords = input<string[]>([]);
  cellClick = output<{x: number, y: number, char: string}>();

  gridCells = computed(() => {
    const lvl = this.level();
    const width = lvl.width;
    const height = lvl.height;

    // Initialize blank grid
    const cells2D: GridCell[][] = [];
    for (let y = 0; y < height; y++) {
      cells2D[y] = [];
      for (let x = 0; x < width; x++) {
        cells2D[y][x] = { char: '', isUsed: false, words: [], x, y };
      }
    }

    // Place words in grid
    lvl.grid.forEach(placement => {
      const word = placement.word.toUpperCase();
      const startX = placement.startX;
      const startY = placement.startY;
      const isHorizontal = placement.direction === 'H';

      for (let i = 0; i < word.length; i++) {
        const x = isHorizontal ? startX + i : startX;
        const y = isHorizontal ? startY : startY + i;

        if (x >= 0 && x < width && y >= 0 && y < height) {
          const cell = cells2D[y][x];
          cell.char = word[i].toUpperCase();
          cell.isUsed = true;
          if (!cell.words.includes(word)) {
            cell.words.push(word);
          }
        }
      }
    });

    // Flatten for rendering in template grid
    const flatCells: GridCell[] = [];
    for (let y = 0; y < height; y++) {
      for (let x = 0; x < width; x++) {
        flatCells.push(cells2D[y][x]);
      }
    }

    return flatCells;
  });

  isCellSolved(cell: GridCell): boolean {
    if (!cell.isUsed) return false;
    
    // Solved if any word it belongs to is solved
    const isWordSolved = cell.words.some(w => this.solvedWords().includes(w));
    if (isWordSolved) return true;

    // Solved if coordinate is revealed via hint
    const coordKey = `${cell.x},${cell.y}`;
    return this.revealedCoords().includes(coordKey);
  }

  onCellClick(cell: GridCell) {
    if (cell.isUsed && this.isCellSolved(cell)) {
      this.cellClick.emit({ x: cell.x, y: cell.y, char: cell.char });
    }
  }
}
