//
//  ConfigManager.swift
//  EpiFib
//
//  Created by Gregory Hewett on 2/26/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

final class ConfigManager: NSObject
{   
    private let useHttps = false
    private let useAzureWebsite = true
    private let apiAzureHostName = "epifibweb.azurewebsites.net/"
    private let apiCustomHostName = "epifib.com/"
    private let apiPath = "api/v1/"
    
    var apiProtocol: String
    {
        get
        {
            return self.useHttps ? "https://" : "http://"
        }
    }
    
    var apiHostName: String
    {
        get
        {
            return self.useAzureWebsite
                ? self.apiAzureHostName
                : self.apiCustomHostName
        }
    }
    
    var apiBase: String
    {
        get
        {
            return self.apiProtocol + self.apiAzureHostName + self.apiPath
        }
    }
}
