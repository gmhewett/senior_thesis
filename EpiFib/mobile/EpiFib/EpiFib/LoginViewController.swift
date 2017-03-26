//
//  LoginController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 2/25/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import KeychainSwift

final class LoginViewController: UIViewController
{
    var authService: AuthService?
    @IBOutlet weak var emailField: UITextField!
    @IBOutlet weak var passwordField: UITextField!
    
    @IBAction func loginButtonPressed(_ sender: Any)
    {
        if self.emailField.text == nil || self.passwordField.text == nil
        {
            print("Don't be stupid\n")
        }
        
        print("Email: \(self.emailField.text!)\nPassword: \(self.passwordField.text!)\n")
        
        self.authService?.login(userName: self.emailField.text!, password: self.passwordField.text!, callback: { r in
            if r.response?.statusCode != 200
            {
                print("You messed up\n")
            }
            
            self.dismiss(animated: true, completion: nil)
        })
    }
}
