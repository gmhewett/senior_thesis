//
//  AuthService.swift
//  EpiFib
//
//  Created by Gregory Hewett on 2/26/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import Alamofire
import KeychainSwift

final class AuthService: NSObject
{
    private let accessTokenKey = "access_token"
    private let userDefaultsKey = "isLoggedIn"
    
    var epiFibApiService: EpiFibApiService?
    let keychain: KeychainSwift
    
    override init() {
        self.keychain = KeychainSwift()
        super.init()
    }
    
    convenience init(_ epiFibApiService: EpiFibApiService)
    {
        self.init()
        self.epiFibApiService = epiFibApiService
    }
    
    var token: String? {
        get {
            return self.keychain.get(self.accessTokenKey)
        }
    }
    
    var isLoggedIn: Bool {
        get {
            return UserDefaults.standard.bool(forKey: self.userDefaultsKey) 
        }
    }
    
    func login(
        userName: String,
        password: String,
        callback: @escaping (DataResponse<Any>) -> Void)
    {
        let parameters = [
            "grant_type" : "password",
            "username" : userName,
            "password" : password
        ]
        
        self.epiFibApiService?.sendRequest(
            endpoint: "Token",
            method: .post,
            parameters: parameters,
            encoding: URLEncoding.httpBody,
            useAuth: false).responseJSON
            { r in
                guard r.response?.statusCode == 200 else {
                    print("Request: \(r.request)\nResponse: \(r.response)\nStatusCode: \(r.response?.statusCode)")
                    return
                }
                
                guard r.result.value != nil else {
                    print("HAHAHA")
                    return
                }
                
                if let json = r.result.value! as? [String : Any]
                {
                    if let access_token = json[self.accessTokenKey] as? String
                    {
                        print("SAVED TOKEN")
                        self.keychain.set(access_token, forKey: self.accessTokenKey)
                        UserDefaults.standard.set(true, forKey: self.userDefaultsKey)
                    }
                }
                callback(r)
            }
    }
    
    func logout()
    {
        self.keychain.clear()
        UserDefaults.standard.removeObject(forKey: "isLoggedIn")
    }
    
    func updateDeviceToken(token: String)
    {
        let parameters = [
            "deviceToken" : token
        ]
        
        self.epiFibApiService?.sendRequest(endpoint: "account/devicetoken", method: .post, parameters: parameters)
            .responseJSON
            { r in
                guard r.response?.statusCode == 200 else {
                    print("Request: \(r.request)\nResponse: \(r.response)\nStatusCode: \(r.response?.statusCode)")
                    return
                }
                print("YEAH")
                
            }
    }
}
