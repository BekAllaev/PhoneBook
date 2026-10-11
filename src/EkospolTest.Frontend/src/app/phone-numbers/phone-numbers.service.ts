import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface PhoneNumber {
  id: number;
  number: string;
  isPublic: boolean;
  ownerId: string;
  ownerUsername: string;
}

export interface CreatePhoneNumberRequest {
  number: string;
  isPublic: boolean;
}

@Injectable({ providedIn: 'root' })
export class PhoneNumbersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/phone-numbers';

  getAll(): Observable<PhoneNumber[]> {
    return this.http.get<PhoneNumber[]>(this.baseUrl);
  }

  create(request: CreatePhoneNumberRequest): Observable<PhoneNumber> {
    return this.http.post<PhoneNumber>(this.baseUrl, request);
  }
}
