//
//  Control.swift
//  Infirmary Integrated
//
//  Created by Ibi Keller on 9/30/25.
//

import SwiftUI

struct Control: View {
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
                Text("Physiology control items")
                
                // TODO: Implement Patient Physiology controls here!
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
