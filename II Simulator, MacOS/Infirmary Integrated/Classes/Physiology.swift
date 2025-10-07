//
//  Physiology.swift
//  Infirmary Integrated
//
//  Created by Ibi Keller on 10/7/25.
//

import Foundation

class Physiology {
    /// Represents a collection of basic and advanced vital signs.
    class VitalSigns {
        // MARK: - Basic vital signs
        var HR: Int = 0                // Heart rate
        var NSBP: Int = 0              // Non-invasive systolic BP
        var NDBP: Int = 0              // Non-invasive diastolic BP
        var NMAP: Int = 0              // Non-invasive mean arterial pressure
        var RR: Int = 0                // Respiratory rate
        var SPO2: Int = 0              // Pulse oximetry
        var T: Double = 0.0            // Temperature
        
        // MARK: - Advanced hemodynamics
        var ETCO2: Int = 0             // End-tidal CO₂
        var CVP: Int = 0               // Central venous pressure
        var ASBP: Int = 0              // Arterial systolic BP
        var ADBP: Int = 0              // Arterial diastolic BP
        var AMAP: Int = 0              // Arterial mean arterial pressure
        
        var PSP: Int = 0               // Pulmonary artery systolic pressure
        var PDP: Int = 0               // Pulmonary artery diastolic pressure
        var PMP: Int = 0               // Pulmonary mean pressure
        
        var ICP: Int = 0               // Intracranial pressure
        var IAP: Int = 0               // Intra-abdominal pressure
        
        var CO: Double = 0.0           // Cardiac output
        
        // MARK: - Respiratory profile
        var RR_IE_I: Double = 0.0      // Inspiratory component
        var RR_IE_E: Double = 0.0      // Expiratory component
        
        var FetalHR: Int = 0           // Baseline fetal heart rate
        
        // MARK: - Initializers
        
        init() { }
        
        init(from v: VitalSigns) {
            set(from: v)
        }
        
        // MARK: - Methods
        
        /// Copies all vital sign values from another instance.
        func set(from v: VitalSigns) {
            HR = v.HR
            NSBP = v.NSBP
            NDBP = v.NDBP
            NMAP = v.NMAP
            RR = v.RR
            SPO2 = v.SPO2
            T = v.T
            
            ETCO2 = v.ETCO2
            CVP = v.CVP
            ASBP = v.ASBP
            ADBP = v.ADBP
            AMAP = v.AMAP
            
            PSP = v.PSP
            PDP = v.PDP
            PMP = v.PMP
            
            ICP = v.ICP
            IAP = v.IAP
            
            CO = v.CO
            
            RR_IE_I = v.RR_IE_I
            RR_IE_E = v.RR_IE_E
            
            FetalHR = v.FetalHR
        }
    }
}
