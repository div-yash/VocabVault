import { Component } from '@angular/core';
import { GameBoard } from './components/game-board/game-board';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [GameBoard],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
}
