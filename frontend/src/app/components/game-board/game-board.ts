import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameService } from '../../services/game.service';
import { Level, Player } from '../../models/game.models';
import { WordGrid } from '../word-grid/word-grid';
import { LetterCircle } from '../letter-circle/letter-circle';
import { MeaningPopup } from '../meaning-popup/meaning-popup';

@Component({
  selector: 'app-game-board',
  standalone: true,
  imports: [CommonModule, WordGrid, LetterCircle, MeaningPopup],
  templateUrl: './game-board.html',
  styleUrl: './game-board.css'
})
export class GameBoard implements OnInit {
  player = signal<Player>({ username: 'Player 1', currentLevel: 1, score: 0 });
  displayedScore = signal<number>(0);
  level = signal<Level | null>(null);
  
  solvedWords = signal<string[]>([]);
  revealedCoords = signal<string[]>([]);
  currentPreview = signal<string>('');
  
  showMeaning = signal<boolean>(false);
  activeWord = signal<string>('');
  activeMeaning = signal<string>('');

  message = signal<string>('');
  messageType = signal<'success' | 'info' | 'error' | ''>('');
  
  shakePreview = signal<boolean>(false);
  successFlash = signal<boolean>(false);
  levelComplete = signal<boolean>(false);

  constructor(private gameService: GameService) {}

  ngOnInit() {
    this.loadPlayerProgress();
  }

  loadPlayerProgress() {
    this.gameService.getPlayerProgress('Player 1').subscribe({
      next: (prog) => {
        this.player.set(prog);
        this.displayedScore.set(prog.score);
        this.loadLevel(prog.currentLevel);
      },
      error: (err) => {
        console.error('Error loading player progress', err);
        this.loadLevel(1);
      }
    });
  }

  loadLevel(levelNumber: number) {
    this.levelComplete.set(false);
    this.solvedWords.set([]);
    this.revealedCoords.set([]);
    this.currentPreview.set('');
    
    this.gameService.getLevel(levelNumber).subscribe({
      next: (lvl) => {
        this.level.set(lvl);
      },
      error: (err) => {
        console.error('Error loading level', err);
        this.showToast('Failed to load level. Please try again.', 'error');
      }
    });
  }

  onWordPreview(word: string) {
    this.currentPreview.set(word);
  }

  onWordFormed(word: string) {
    this.currentPreview.set('');
    const lvl = this.level();
    if (!lvl) return;

    const guess = word.toUpperCase();
    
    if (lvl.words.some(w => w.toUpperCase() === guess)) {
      if (this.solvedWords().includes(guess)) {
        this.showToast('Already found!', 'info');
        this.triggerShake();
      } else {
        const updatedSolved = [...this.solvedWords(), guess];
        this.solvedWords.set(updatedSolved);
        
        const points = guess.length * 10;
        const currentProg = this.player();
        const newScore = currentProg.score + points;
        const updatedPlayer = {
          ...currentProg,
          score: newScore
        };
        this.player.set(updatedPlayer);
        this.gameService.updatePlayerProgress(updatedPlayer).subscribe();
        this.animateScore(newScore);
        this.triggerConfetti();

        this.successFlash.set(true);
        setTimeout(() => this.successFlash.set(false), 500);

        this.showToast(`+${points} Points!`, 'success');

        this.fetchAndShowMeaning(guess);

        if (updatedSolved.length === lvl.words.length) {
          this.triggerLevelComplete();
        }
      }
    } else {
      this.showToast('Invalid word!', 'error');
      this.triggerShake();
    }
  }

  fetchAndShowMeaning(word: string) {
    this.gameService.getMeaning(word).subscribe({
      next: (res) => {
        this.activeWord.set(res.word);
        this.activeMeaning.set(res.meaning);
        this.showMeaning.set(true);
      },
      error: (err) => {
        console.error('Error fetching meaning', err);
        this.activeWord.set(word);
        this.activeMeaning.set('Meaning unavailable at this time.');
        this.showMeaning.set(true);
      }
    });
  }

  onCellClick(event: { x: number, y: number, char: string }) {
    const lvl = this.level();
    if (!lvl) return;

    const cellWord = lvl.grid.find(p => {
      const isHorizontal = p.direction === 'H';
      const wLen = p.word.length;
      if (isHorizontal) {
        return event.y === p.startY && event.x >= p.startX && event.x < p.startX + wLen;
      } else {
        return event.x === p.startX && event.y >= p.startY && event.y < p.startY + wLen;
      }
    });

    if (cellWord && this.solvedWords().includes(cellWord.word.toUpperCase())) {
      this.fetchAndShowMeaning(cellWord.word);
    }
  }

  useHint() {
    const lvl = this.level();
    if (!lvl) return;

    const unsolvedCells: { x: number, y: number }[] = [];
    const solved = this.solvedWords();
    const revealed = this.revealedCoords();

    const cellMap = new Map<string, { x: number, y: number, char: string, words: string[] }>();
    
    lvl.grid.forEach(p => {
      const isH = p.direction === 'H';
      for (let i = 0; i < p.word.length; i++) {
        const cx = isH ? p.startX + i : p.startX;
        const cy = isH ? p.startY : p.startY + i;
        const key = `${cx},${cy}`;
        
        if (!cellMap.has(key)) {
          cellMap.set(key, { x: cx, y: cy, char: p.word[i], words: [] });
        }
        cellMap.get(key)!.words.push(p.word.toUpperCase());
      }
    });

    for (const [key, cell] of cellMap.entries()) {
      const isSolved = cell.words.some(w => solved.includes(w)) || revealed.includes(key);
      if (!isSolved) {
        unsolvedCells.push({ x: cell.x, y: cell.y });
      }
    }

    if (unsolvedCells.length === 0) return;

    const currentProg = this.player();
    if (currentProg.score < 50 && currentProg.currentLevel === 1 && currentProg.score === 0) {
      // Free hint for Level 1 starting out
    } else if (currentProg.score < 50) {
      this.showToast('Requires 50 points for a hint!', 'error');
      return;
    } else {
      const newScore = currentProg.score - 50;
      const updatedPlayer = {
        ...currentProg,
        score: newScore
      };
      this.player.set(updatedPlayer);
      this.gameService.updatePlayerProgress(updatedPlayer).subscribe();
      this.animateScore(newScore);
    }

    const randCell = unsolvedCells[Math.floor(Math.random() * unsolvedCells.length)];
    const coordKey = `${randCell.x},${randCell.y}`;
    this.revealedCoords.set([...revealed, coordKey]);
    
    this.showToast('Hint revealed!', 'info');

    const allCellsRevealed = Array.from(cellMap.keys()).every(key => 
      cellMap.get(key)!.words.some(w => solved.includes(w)) || 
      [...revealed, coordKey].includes(key)
    );

    if (allCellsRevealed) {
      const allWords = lvl.words.map(w => w.toUpperCase());
      this.solvedWords.set(allWords);
      this.triggerLevelComplete();
    }
  }

  triggerLevelComplete() {
    this.levelComplete.set(true);
    this.showToast('Level Complete! Fantastic!', 'success');
    
    const currentProg = this.player();
    const updatedPlayer = {
      ...currentProg,
      currentLevel: currentProg.currentLevel + 1
    };
    
    this.player.set(updatedPlayer);
    this.gameService.updatePlayerProgress(updatedPlayer).subscribe();
    
    // Double confetti explosion for level complete!
    this.triggerConfetti();
    setTimeout(() => this.triggerConfetti(), 350);
  }

  nextLevel() {
    this.loadLevel(this.player().currentLevel);
  }

  restartLevel() {
    this.loadLevel(this.player().currentLevel);
  }

  triggerShake() {
    this.shakePreview.set(true);
    setTimeout(() => this.shakePreview.set(false), 500);
  }

  showToast(msg: string, type: 'success' | 'info' | 'error') {
    this.message.set(msg);
    this.messageType.set(type);
    setTimeout(() => {
      if (this.message() === msg) {
        this.message.set('');
        this.messageType.set('');
      }
    }, 3000);
  }

  animateScore(targetScore: number) {
    const start = this.displayedScore();
    const duration = 600; // ms
    const startTime = performance.now();
    
    const updateScore = (now: number) => {
      const elapsed = now - startTime;
      const progress = Math.min(elapsed / duration, 1);
      const easedProgress = progress * (2 - progress); // easeOutQuad
      const current = Math.floor(start + (targetScore - start) * easedProgress);
      this.displayedScore.set(current);
      
      if (progress < 1) {
        requestAnimationFrame(updateScore);
      } else {
        this.displayedScore.set(targetScore);
      }
    };
    
    requestAnimationFrame(updateScore);
  }

  triggerConfetti() {
    const colors = ['#00f2fe', '#4facfe', '#ff9f43', '#ff5252', '#2ed573', '#1e90ff', '#fffa65'];
    const container = document.body;
    const particleCount = 40;
    
    // Position explosion in the center of the viewport
    const centerX = window.innerWidth / 2;
    const centerY = window.innerHeight * 0.45;
    
    for (let i = 0; i < particleCount; i++) {
      const particle = document.createElement('div');
      particle.className = 'confetti-particle';
      
      const size = Math.random() * 8 + 6;
      const color = colors[Math.floor(Math.random() * colors.length)];
      const angle = Math.random() * Math.PI * 2;
      const velocity = Math.random() * 8 + 4;
      const velocityX = Math.cos(angle) * velocity;
      const velocityY = Math.sin(angle) * velocity;
      
      particle.style.width = `${size}px`;
      particle.style.height = `${size}px`;
      particle.style.backgroundColor = color;
      particle.style.left = `${centerX}px`;
      particle.style.top = `${centerY}px`;
      particle.style.position = 'fixed';
      particle.style.borderRadius = Math.random() > 0.5 ? '50%' : '0%';
      particle.style.pointerEvents = 'none';
      particle.style.zIndex = '9999';
      particle.style.transform = `rotate(${Math.random() * 360}deg)`;
      
      container.appendChild(particle);
      
      let posX = centerX;
      let posY = centerY;
      let currentVelocityY = velocityY;
      let opacity = 1;
      
      const updateAnimation = () => {
        posX += velocityX;
        posY += currentVelocityY;
        currentVelocityY += 0.22; // gravity
        opacity -= 0.018; // fade out
        
        particle.style.left = `${posX}px`;
        particle.style.top = `${posY}px`;
        particle.style.opacity = `${opacity}`;
        
        if (opacity > 0) {
          requestAnimationFrame(updateAnimation);
        } else {
          particle.remove();
        }
      };
      
      requestAnimationFrame(updateAnimation);
    }
  }
}
