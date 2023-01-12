import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';

import { AccountService } from 'src/app/_services/account.service';
import { User } from '../_models/user';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};  
  
  constructor(public accountService: AccountService ) { }

  ngOnInit(): void {}
  
  login() {
    this.accountService.login(this.model)
      .subscribe({
        next: response => {
          console.log(response);
        },
        error: error => console.error(error)
      });
  }
  
  logout() {
    this.accountService.logout();
  }
}
