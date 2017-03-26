//
//  EpiFibApi.swift
//  EpiFib
//
//  Created by Gregory Hewett on 2/26/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import Alamofire
import SwinjectStoryboard

final class EpiFibApiService: NSObject
{
    let authService: AuthService
    var apiBase: String
    
    private lazy  var  backgroundManager: SessionManager =
    {
        let configuration = URLSessionConfiguration.background(withIdentifier: "com.thereachlab.epifib.background")
        return Alamofire.SessionManager(configuration: configuration)
    }()
    
    private lazy var defaultManager: SessionManager =
    {
        let configuration = URLSessionConfiguration.default
        return Alamofire.SessionManager(configuration: configuration)
    }()
        
    init(_ configManager: ConfigManager, _ authService: AuthService)
    {
        self.apiBase = configManager.apiBase
        self.authService = authService
    }
    
    func sendRequest(
        endpoint: String,
        method: HTTPMethod,
        parameters: [String : Any]?,
        encoding: ParameterEncoding = JSONEncoding.default,
        useAuth: Bool = true) -> DataRequest
    {
        var headers: HTTPHeaders = [:]
        
        if useAuth
        {
            if let token = self.authService.token {
                headers["Authorization"] = "Bearer \(token)"
            }
        }
        
        print(apiBase)
        print(endpoint)
        
        let manager: SessionManager = UIApplication.shared.applicationState == .background
            ? self.backgroundManager
            : self.defaultManager
        
        if parameters != nil
        {
            return manager.request(
                self.apiBase + endpoint,
                method: method,
                parameters: parameters,
                encoding: encoding,
                headers: headers)
        }
        else
        {
            return manager.request(
                self.apiBase + endpoint,
                method: method,
                parameters: parameters,
                encoding: encoding,
                headers: headers)
        }
    }
}
