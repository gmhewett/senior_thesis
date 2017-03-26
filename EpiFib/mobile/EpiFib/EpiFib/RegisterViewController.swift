//
//  RegisterController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 2/26/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import Alamofire
import KeychainSwift

final class RegisterViewController: UIViewController
{
    var authService: AuthService?
    
    required init?(coder aDecoder: NSCoder)
    {
        super.init(coder: aDecoder)
    }
    
    @IBOutlet weak var emailField: UITextField!
    
    @IBOutlet weak var passwordField: UITextField!
    
    @IBOutlet weak var confPasswordField: UITextField!
    
    override func viewDidLoad() {
        super.viewDidLoad()
    }
    
    @IBAction func registerButtonPressed(_ sender: Any)
    {
        self.authService!.login(userName: "gregory.hewett@me.com", password: "Elah#5920") { _ in
            self.dismiss(animated: true, completion: nil)
        }

    }
    
    @IBAction func cancelButtonPressed(_ sender: Any)
    {
        self.dismiss(animated: true, completion: nil)
    }
}
