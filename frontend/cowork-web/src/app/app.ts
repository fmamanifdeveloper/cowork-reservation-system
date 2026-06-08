import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationBanner } from './shared/ui/notification-banner/notification-banner';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NotificationBanner],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  
}
