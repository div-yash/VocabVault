import { Component, computed, input, output, ElementRef, viewChild, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

interface LetterPos {
  char: string;
  index: number;
  x: number;
  y: number;
}

@Component({
  selector: 'app-letter-circle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './letter-circle.html',
  styleUrl: './letter-circle.css'
})
export class LetterCircle {
  letters = input.required<string>();
  wordFormed = output<string>();
  wordPreview = output<string>();

  svgContainer = viewChild<ElementRef<SVGSVGElement>>('svgContainer');

  selectedIndices: number[] = [];
  isDrawing = false;
  pointerX = 0;
  pointerY = 0;

  letterPositions = computed(() => {
    const l = this.letters();
    const len = l.length;
    const center = 150;
    const radius = 95;

    return l.split('').map((char, index) => {
      const angle = (index * (360 / len) - 90) * (Math.PI / 180);
      return {
        char: char.toUpperCase(),
        index,
        x: center + radius * Math.cos(angle),
        y: center + radius * Math.sin(angle)
      };
    });
  });

  get currentWord(): string {
    const l = this.letters();
    return this.selectedIndices.map(idx => l[idx]).join('').toUpperCase();
  }

  onStart(index: number, event: MouseEvent | TouchEvent) {
    event.preventDefault();
    this.isDrawing = true;
    this.selectedIndices = [index];
    this.updatePointerCoords(event);
    this.wordPreview.emit(this.currentWord);
  }

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent) {
    if (!this.isDrawing) return;
    this.updatePointerCoords(event);
    this.checkCollisions();
  }

  @HostListener('document:touchmove', ['$event'])
  onTouchMove(event: TouchEvent) {
    if (!this.isDrawing) return;
    this.updatePointerCoords(event);
    this.checkCollisions();
  }

  @HostListener('document:mouseup', ['$event'])
  onMouseUp(event: MouseEvent) {
    this.endSelection();
  }

  @HostListener('document:touchend', ['$event'])
  onTouchEnd(event: TouchEvent) {
    this.endSelection();
  }

  private updatePointerCoords(event: MouseEvent | TouchEvent) {
    const svgEl = this.svgContainer()?.nativeElement;
    if (!svgEl) return;

    const rect = svgEl.getBoundingClientRect();
    let clientX = 0;
    let clientY = 0;

    if ('touches' in event) {
      if (event.touches.length > 0) {
        clientX = event.touches[0].clientX;
        clientY = event.touches[0].clientY;
      } else if (event.changedTouches.length > 0) {
        clientX = event.changedTouches[0].clientX;
        clientY = event.changedTouches[0].clientY;
      }
    } else {
      clientX = event.clientX;
      clientY = event.clientY;
    }

    this.pointerX = ((clientX - rect.left) / rect.width) * 300;
    this.pointerY = ((clientY - rect.top) / rect.height) * 300;
  }

  private checkCollisions() {
    const collisionRadius = 26;
    const positions = this.letterPositions();

    for (const pos of positions) {
      const dx = this.pointerX - pos.x;
      const dy = this.pointerY - pos.y;
      const dist = Math.sqrt(dx * dx + dy * dy);

      if (dist < collisionRadius) {
        const index = pos.index;
        const selectedLen = this.selectedIndices.length;

        if (selectedLen > 1 && this.selectedIndices[selectedLen - 2] === index) {
          this.selectedIndices.pop();
          this.wordPreview.emit(this.currentWord);
        } else if (!this.selectedIndices.includes(index)) {
          this.selectedIndices.push(index);
          this.wordPreview.emit(this.currentWord);
        }
        break;
      }
    }
  }

  private endSelection() {
    if (!this.isDrawing) return;
    this.isDrawing = false;
    const finalWord = this.currentWord;
    
    if (finalWord.length >= 3) {
      this.wordFormed.emit(finalWord);
    } else {
      this.wordPreview.emit('');
    }
    
    this.selectedIndices = [];
  }

  get svgPath(): string {
    if (this.selectedIndices.length === 0) return '';
    const positions = this.letterPositions();
    return this.selectedIndices.map((idx, index) => {
      const pos = positions[idx];
      return `${index === 0 ? 'M' : 'L'} ${pos.x} ${pos.y}`;
    }).join(' ');
  }

  get lastSelectedPos(): LetterPos | null {
    if (this.selectedIndices.length === 0) return null;
    const idx = this.selectedIndices[this.selectedIndices.length - 1];
    return this.letterPositions()[idx];
  }

  isLetterSelected(index: number): boolean {
    return this.selectedIndices.includes(index);
  }

  getSelectionOrder(index: number): number {
    return this.selectedIndices.indexOf(index) + 1;
  }
}
