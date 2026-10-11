import { Component, inject, OnInit, signal } from '@angular/core';
import { PhoneNumber, PhoneNumbersService } from './phone-numbers.service';

@Component({
  selector: 'app-phone-numbers',
  standalone: false,
  templateUrl: './phone-numbers.html',
  styleUrl: './phone-numbers.css',
})
export class PhoneNumbers implements OnInit {
  private readonly phoneNumbersService = inject(PhoneNumbersService);

  protected readonly phoneNumbers = signal<PhoneNumber[]>([]);
  protected readonly error = signal<string | null>(null);

  protected number = '';
  protected isPublic = false;

  ngOnInit(): void {
    this.load();
  }

  protected add(): void {
    this.error.set(null);
    this.phoneNumbersService.create({ number: this.number, isPublic: this.isPublic }).subscribe({
      next: (created) => {
        this.phoneNumbers.update((list) => [...list, created]);
        this.number = '';
        this.isPublic = false;
      },
      error: () => this.error.set('Číslo se nepodařilo přidat.'),
    });
  }

  private load(): void {
    this.phoneNumbersService.getAll().subscribe({
      next: (list) => this.phoneNumbers.set(list),
      error: () => this.error.set('Čísla se nepodařilo načíst.'),
    });
  }
}
