import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { App } from './app';
import { IssuesHubService } from './generated/IssuesHub';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        {
          provide: IssuesHubService,
          useValue: {
            connected: signal(false),
            issueCreated: signal(null),
            issueAssigned: signal(null),
            issueUnassigned: signal(null),
            issueClosed: signal(null),
            issueOpened: signal(null),
            allEvents: signal([]),
            subscribeToIssue: () => Promise.resolve(),
            unsubscribeFromIssue: () => Promise.resolve(),
          },
        },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});
