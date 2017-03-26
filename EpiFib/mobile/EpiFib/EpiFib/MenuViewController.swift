//
//  MenuViewController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/18/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit

final class MenuViewController: UITableViewController
{
    var authService: AuthService?
    
    @IBAction func logoutButtonPressed(_ sender: Any)
    {
        self.authService?.logout()
        self.performSegue(withIdentifier: "SegueToHomeScreen", sender: self)
    }
}
