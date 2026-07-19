import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-meaning-popup',
  standalone: true,
  templateUrl: './meaning-popup.html',
  styleUrl: './meaning-popup.css'
})
export class MeaningPopup {
  word = input.required<string>();
  meaning = input.required<string>();
  close = output<void>();

  onClose() {
    this.close.emit();
  }
}
