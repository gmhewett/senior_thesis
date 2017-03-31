//
//  EmergencyService.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/17/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import Foundation
import Alamofire

protocol EmergencyServiceDelegate
{
    func didUpdateEmergencyInstance(_ ownerPacket: EmergencyOwnerPacket)
}


final class EmergencyService: NSObject
{
    var delegate: EmergencyServiceDelegate?
    var epiFibApiService: EpiFibApiService
    var locationService: LocationService
    var currentEmergencyInstanceRequests: [EmergencyInstanceRequest]?
    
    init(_ epiFibApiService: EpiFibApiService, _ locationService: LocationService)
    {
        self.epiFibApiService = epiFibApiService
        self.locationService = locationService
    }
    
    func createEmergencyInstance(type: EmergencyType, completion: @escaping (EmergencyOwnerPacket?, Error?) -> Void)
    {
        let emergencyInstanceRequest = EmergencyInstanceRequest()
        emergencyInstanceRequest.emergencyType =  type
        emergencyInstanceRequest.ownerLocation = self.locationService.currentLocation!
        
        let params = emergencyInstanceRequest.toDictionary() as! [String : Any]
        
        self.epiFibApiService.sendRequest(endpoint: "emergency/create", method: .post, parameters: params)
            .responseJSON { (jsonResponse) in
                completion(jsonResponse.result.value as? EmergencyOwnerPacket, jsonResponse.error)
        }
    }
    
    func getCurrentEmergency(completion: @escaping (EmergencyOwnerPacket?, Error?) -> Void)
    {
        self.epiFibApiService.sendRequest(endpoint: "emergency/current", method: .get, parameters: nil)
            .responseObject { (response: DataResponse<EmergencyOwnerPacket>) in
                if response.response?.statusCode != 200
                {
                    print(response.response?.statusCode ?? "NO RESPONSE")
                    print(response.result.value ?? "NO DATA")
                }
                else
                {
                    print(response.result.value ?? "NO DATA")
                    completion(response.result.value, response.error)
                }
        }
    }
    
    func toggleContainer(on: Bool, containerId: String, emergencyInstanceId: String)
    {
        let parameters: [String : Any] = [
            "EmergencyInstanceId" : emergencyInstanceId,
            "ContainerId" : containerId,
            "Value" : on ? 1 : 0
        ]
        
        self.epiFibApiService.sendRequest(endpoint: "emergency/container", method: .post, parameters: parameters)
        .responseJSON { (response) in
            print(response)
        }
    }
    
    func checkIfHelpNeeded(completion: @escaping (EmergencyInstanceRequest?, Error?) -> Void)
    {
        self.epiFibApiService.sendRequest(endpoint: "emergency/nearby", method: .get, parameters: nil)
            .responseObject { (response: DataResponse<EmergencyInstanceRequest>) in
                if response.response?.statusCode != 200
                {
                    print(response.response?.statusCode ?? "NO RESPONSE")
                    print(response.result.value ?? "NO DATA")
                }
                else
                {
                    print(response.result.value ?? "NO DATA")
                    completion(response.result.value, response.error)
                }
        }
    }
    
    func respondToEmergency(emergencyInstanceId: String, completion: @escaping (EmergencyResponderPacket?, Error?) -> Void)
    {
        self.epiFibApiService.sendRequest(endpoint: "emergency/nearby", method: .get, parameters: nil)
            .responseObject { (response: DataResponse<EmergencyResponderPacket>) in
                if response.response?.statusCode != 200
                {
                    print(response.response?.statusCode ?? "NO RESPONSE")
                    print(response.result.value ?? "NO DATA")
                }
                else
                {
                    print(response.result.value ?? "NO DATA")
                    completion(response.result.value, response.error)
                }
            }
    }
}
