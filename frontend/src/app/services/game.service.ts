import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Level, Player, WordMeaning } from '../models/game.models';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly apiUrl: string;

  constructor(private http: HttpClient) {
    const isLocal = typeof window !== 'undefined' && window.location.hostname === 'localhost';
    this.apiUrl = isLocal 
      ? 'http://localhost:5194/api' 
      : 'https://vocabvault-api.onrender.com/api';
  }

  getLevel(levelNumber: number): Observable<Level> {
    return this.http.get<Level>(`${this.apiUrl}/level/${levelNumber}`);
  }

  getMeaning(word: string): Observable<WordMeaning> {
    return this.http.get<WordMeaning>(`${this.apiUrl}/meaning/${word}`);
  }

  getPlayerProgress(username: string): Observable<Player> {
    return this.http.get<Player>(`${this.apiUrl}/player/${username}`);
  }

  updatePlayerProgress(player: Player): Observable<Player> {
    return this.http.post<Player>(`${this.apiUrl}/player/update`, player);
  }
}
