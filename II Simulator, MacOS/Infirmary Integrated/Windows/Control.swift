//
//  Control.swift
//  Infirmary Integrated
//
//  Created by Ibi Keller on 9/30/25.
//

import SwiftUI

import Foundation

struct Control: View {
    @State private var _vs = Physiology.VitalSigns()
    
    @State private var _cardiacRhythm = "Sinus Rhythm"
    @State private var _respiratoryRhythm = "Regular"
    
    let _cardiacRhythms = [ "Asystole", "Sinus Rhythm", "Ventricular Fibrillation" ]
    let _respiratoryRhythms = [ "Apnea", "Cheyne Stokes", "Regular" ]
    
    
    var body: some View {
        NavigationView {
            List {
                // TODO: Implement Device List here!
                Text("Cardiac Monitor")
                Text("Defibrillator")
                Text("12 Lead ECG")
                Text("Intra-aortic Balloon Pump")
                Text("External Fetal Monitor")
            }
            VStack {
                /* TODO: Implement:
                 *   - Replace all `private var` variables with the Physiology class
                 *   - Replace hard-coded limits with the same limits from IISIM Avalonia
                 */
                
                GroupBox(label: Text("Vital Signs").font(.title3).bold()) {
                    HStack {
                        Text("Heart Rate:")
                            .frame(maxWidth: .infinity, alignment: .leading)
                        
                        Stepper("\(_vs.HR)", value: $_vs.HR, in: 0...200, step: 5)
                            .frame(maxWidth: .infinity, alignment: .trailing)
                    }
                    
                    HStack {
                        Text("Blood Pressure:")
                            .frame(maxWidth: .infinity, alignment: .leading)
                        
                        HStack{
                            Stepper("\(_vs.NSBP)", value: $_vs.NSBP, in: 0...300, step: 5)
                                .frame(maxWidth: .infinity, alignment: .leading)
                            
                            Text("/")
                            
                            Stepper("\(_vs.NDBP)", value: $_vs.NDBP, in: 0...300, step: 5)
                                .frame(maxWidth: .infinity, alignment: .trailing)
                        }
                    }
                    
                    HStack {
                        Text("Respiratory Rate:")
                            .frame(maxWidth: .infinity, alignment: .leading)
                        
                        Stepper("\(_vs.RR)", value: $_vs.RR, in: 0...100, step: 1)
                            .frame(maxWidth: .infinity, alignment: .trailing)
                    }
                    
                    HStack {
                        Text("Pulse Oximetry:")
                            .frame(maxWidth: .infinity, alignment: .leading)
                        
                        Stepper("\(_vs.SPO2)", value: $_vs.SPO2, in: 0...100, step: 1)
                            .frame(maxWidth: .infinity, alignment: .trailing)
                    }
                    
                    HStack {
                        Text("Temperature:")
                            .frame(maxWidth: .infinity, alignment: .leading)
                        
                        Stepper("\(_vs.T, specifier: "%.1f")", value: $_vs.T, in: 0...100, step: 0.2)
                            .frame(maxWidth: .infinity, alignment: .trailing)
                    }
                    
                    HStack {
                        Text("Cardiac Rhythm:")
                            .frame(maxWidth: .infinity, alignment: .leading)
                        
                        Picker("", selection: $_cardiacRhythm) {
                            ForEach(_cardiacRhythms, id: \.self) { ea in
                                Text(ea)
                            }
                        }
                        .pickerStyle(.menu)
                        .frame(maxWidth: .infinity, alignment: .trailing)
                    }
                    
                    HStack {
                        Text("Respiratory Rhythm:")
                            .frame(maxWidth: .infinity, alignment: .leading)
                        
                        Picker("", selection: $_respiratoryRhythm) {
                            ForEach(_respiratoryRhythms, id: \.self) { ea in
                                Text(ea)
                            }
                        }
                        .pickerStyle(.menu)
                        .frame(maxWidth: .infinity, alignment: .trailing)
                    }
                }
            }
        }
        
        .toolbar {
            ToolbarItem(placement: .navigation) {
                Button (action: {
                    // TODO: Implement
                }) {
                        Image(systemName: "sidebar.left")
                }
            }
            
            ToolbarItem(placement: .status) {
                Button (action: {
                    // TODO: Implement
                }) {
                        Image(systemName: "speaker")
                }
            }
            ToolbarItem(placement: .status) {
                Button (action: {
                    // TODO: Implement
                }) {
                        Image(systemName: "person.icloud")
                }
            }
        }
    }
}

struct Control_Previews: PreviewProvider {
    static var previews: some View {
        Control()
    }
}
