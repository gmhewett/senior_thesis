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
                    completion(response.result.value, response.error)
                }
        }

//            .responseJSON { (jsonResponse<EmergencyOwnerPacket>) in
//                if jsonResponse.response?.statusCode != 200
//                {
//                    print(jsonResponse.response?.statusCode ?? "NO RESPONSE")
//                    print(jsonResponse.result.value ?? "NO DATA")
//                }
//                else
//                {
////                    let a = jsonResponse.result.value as! EmergencyOwnerPacket
////                    print(jsonResponse)
//                    completion(jsonResponse.result.value as? EmergencyOwnerPacket, jsonResponse.error)
//                }
//            }
    }
}
