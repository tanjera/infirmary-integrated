using System;
using System.Collections.Generic;

namespace II.Waveform {
	public static partial class Dictionary {
		public static Plot ABP_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.12, 0.24, 0.34, 0.43, 0.51, 0.58, 0.65, 0.7, 0.74, 0.77, 0.79, 0.8, 0.8, 0.8, 
				0.79, 0.79, 0.78, 0.78, 0.77, 0.76, 0.75, 0.74, 0.72, 0.71, 0.69, 0.65, 0.61, 0.57, 0.54, 
				0.51, 0.48, 0.46, 0.44, 0.42, 0.41, 0.4, 0.4, 0.4, 0.39, 0.38, 0.37, 0.35, 0.33, 0.31, 
				0.28, 0.24, 0.21, 0.17, 0.12, 0.1, 0.1, 0.1, 0.09, 0.09, 0.08, 0.08, 0.08, 0.07, 0.07, 
				0.06, 0.06, 0.06, 0.05, 0.05, 0.04, 0.04, 0.04, 0.03, 0.03, 0.02, 0.02, 0.02, 0.01, 0.01, 
				0
			}
		};

		public static Plot CVP_Atrioventricular = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.01, 0.01, 0.02, 0.02, 0.06, 0.11, 0.14, 0.17, 0.2, 0.23, 0.25, 0.28, 0.29, 0.32, 0.34, 
				0.36, 0.38, 0.4, 0.42, 0.44, 0.45, 0.46, 0.47, 0.47, 0.48, 0.49, 0.49, 0.49, 0.48, 0.48, 
				0.48, 0.47, 0.46, 0.45, 0.44, 0.43, 0.43, 0.41, 0.41, 0.4, 0.39, 0.39, 0.38, 0.38, 0.39, 
				0.42, 0.46, 0.48, 0.51, 0.54, 0.56, 0.58, 0.61, 0.63, 0.65, 0.68, 0.71, 0.74, 0.77, 0.79, 
				0.8, 0.8, 0.77, 0.73, 0.7, 0.67, 0.65, 0.61, 0.58, 0.52, 0.51, 0.5, 0.49, 0.48, 0.47, 
				0.46, 0.45, 0.42, 0.4, 0.37, 0.33, 0.3, 0.24, 0.19, 0.16, 0.11, 0.07, 0.04, 0.01, -0.03, 
				-0.04, -0.05, -0.05, -0.05, -0.01
			}
		};

		public static Plot CVP_Ventricular = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				-0.01, -0.01, -0.01, -0.01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
				0, 0, 0, 0, -0.01, -0.01, -0.01, -0.01, -0.01, -0.01, -0.01, -0.01, -0.01, 0, 0, 
				0, 0, 0, 0, 0, 0, 0, 0.01, 0.03, 0.06, 0.14, 0.21, 0.26, 0.29, 0.37, 
				0.41, 0.46, 0.5, 0.54, 0.56, 0.58, 0.6, 0.63, 0.65, 0.68, 0.7, 0.73, 0.76, 0.79, 0.81, 
				0.81, 0.78, 0.76, 0.72, 0.7, 0.66, 0.63, 0.6, 0.58, 0.5, 0.49, 0.48, 0.48, 0.46, 0.46, 
				0.44, 0.42, 0.38, 0.35, 0.32, 0.27, 0.22, 0.18, 0.13, 0.08, 0.03, 0, -0.01, -0.02, -0.02, 
				-0.03, -0.03, -0.03, -0.02, 0
			}
		};

		public static Plot ECG_Complex_Idioventricular = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.26, 0.52, 0.84, 0.81, 0.78, 0.61, 0.02, -0.24, -0.33, -0.45, -0.45, -0.41, -0.38, -0.34, 
				-0.31, -0.31, -0.33, -0.34, -0.38, -0.38, -0.37, -0.31, -0.25, -0.11, 0.01, 0.06, 0.09, 0.11, 0.13, 
				0.12, 0.11, 0.1, 0.09, 0.09, 0.08, 0.07, 0.07, 0.05, 0.05, 0.05, 0.05, 0.05, 0.04, 0.04, 
				0.03, 0.03, 0.03, 0.03, 0.02, 0.02, 0.02, 0.01, 0.01, 0.01, 0, 0, 0, 0, 0, 
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			}
		};

		public static Plot ECG_Complex_VT = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				-0.02, -0.03, -0.04, -0.05, -0.06, -0.07, -0.08, -0.09, -0.09, -0.1, -0.1, -0.1, -0.1, -0.1, -0.11, 
				-0.11, -0.12, -0.12, -0.13, -0.14, -0.15, -0.16, -0.18, -0.19, -0.2, -0.32, -0.44, -0.54, -0.63, -0.71, 
				-0.78, -0.85, -0.9, -0.94, -0.97, -0.99, -1, -1, -0.98, -0.96, -0.93, -0.89, -0.84, -0.78, -0.71, 
				-0.64, -0.55, -0.46, -0.35, -0.3, -0.25, -0.19, -0.14, -0.09, -0.05, 0, 0.04, 0.08, 0.11, 0.15, 
				0.18, 0.21, 0.24, 0.26, 0.29, 0.31, 0.33, 0.35, 0.36, 0.37, 0.38, 0.39, 0.4, 0.4, 0.4, 
				0.4, 0.4, 0.4, 0.39, 0.39, 0.38, 0.38, 0.37, 0.36, 0.35, 0.34, 0.33, 0.32, 0.31, 0.29, 
				0.28, 0.26, 0.24, 0.23, 0.21, 0.19, 0.17, 0.15, 0.12, 0.1, 0.1
			}
		};

		public static Plot ECG_CPR_Artifact = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, -0.03, -0.04, -0.02, 0, 0.01, 0.04, 0.06, 0.06, 0.04, -0.02, -0.08, -0.14, -0.13, -0.14, 
				-0.16, -0.16, -0.18, -0.18, -0.18, -0.15, -0.11, -0.07, -0.02, 0.01, 0.04, 0.06, 0.07, 0.09, 0.1, 
				0.11, 0.16, 0.18, 0.19, 0.2, 0.23, 0.23, 0.53, 0.68, 0.77, 0.86, 0.95, 0.99, 1, 1, 
				1, 1, 1, 0.99, 1, 0.99, 0.97, 0.93, 0.87, 0.82, 0.7, 0.65, 0.61, 0.58, 0.55, 
				0.52, 0.46, 0.32, 0.23, 0.16, 0.07, 0.01, -0.02, 0, -0.02, -0.03, 0.02, 0.03, 0.04, 0.02, 
				0
			}
		};

		public static Plot ECG_Defibrillation = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 1, 1, 0.95, 0.9, 0.85, 0.8, 0.75, 0.7, 0.65, 0.6, 0.55, 0.5, 0.5, -0.5, 
				-0.5, -0.44, -0.38, -0.31, -0.25, -0.19, -0.12, -0.06, 0
			}
		};

		public static Plot ECG_Pacemaker = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.1, 0.2, 0.2, 0.1, 0, 0, 0, 0
			}
		};

		public static Plot ETCO2_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.11, 0.22, 0.31, 0.38, 0.45, 0.5, 0.55, 0.59, 0.6, 0.62, 0.63, 0.64, 0.64, 0.64, 0.65, 
				0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 
				0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.65, 0.66, 0.66, 0.66, 0.66, 0.66, 
				0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 
				0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 0.66, 
				0.66, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 
				0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 
				0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 
				0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 
				0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.68, 0.69, 
				0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 
				0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 0.69, 
				0.69, 0.69, 0.69, 0.69, 0.69, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 
				0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 
				0.7, 0.7, 0.7, 0.69, 0.67, 0.64, 0.59, 0.52, 0.45, 0.36, 0.25, 0.13, 0, 0
			}
		};

		public static Plot IABP_ABP_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.12, 0.24, 0.34, 0.43, 0.51, 0.58, 0.65, 0.7, 0.74, 0.77, 0.79, 0.8, 0.8, 0.79, 
				0.79, 0.78, 0.77, 0.75, 0.74, 0.72, 0.7, 0.67, 0.65, 0.62, 0.6, 0.66, 0.72, 0.77, 0.82, 
				0.86, 0.89, 0.92, 0.95, 0.97, 0.98, 0.99, 1, 0.99, 0.98, 0.95, 0.93, 0.9, 0.87, 0.85, 
				0.81, 0.81, 0.81, 0.81, 0.81, 0.81, 0.81, 0.8, 0.8, 0.8, 0.79, 0.78, 0.77, 0.76, 0.75, 
				0.74, 0.72, 0.69, 0.67, 0.65, 0.62, 0.57, 0.5, 0.43, 0.36, 0.25, -0.06, -0.09, -0.08, -0.07, 
				0
			}
		};

		public static Plot IABP_ABP_Ectopic = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.06, 0.12, 0.17, 0.22, 0.26, 0.29, 0.32, 0.35, 0.37, 0.38, 0.39, 0.4, 0.4, 0.4, 
				0.39, 0.39, 0.38, 0.38, 0.37, 0.36, 0.35, 0.34, 0.32, 0.31, 0.3, 0.38, 0.45, 0.51, 0.57, 
				0.62, 0.66, 0.7, 0.74, 0.76, 0.78, 0.79, 0.8, 0.79, 0.78, 0.75, 0.72, 0.67, 0.62, 0.57, 
				0.56, 0.55, 0.54, 0.54, 0.54, 0.54, 0.54, 0.54, 0.54, 0.54, 0.54, 0.54, 0.54, 0.53, 0.53, 
				0.53, 0.52, 0.51, 0.5, 0.49, 0.48, 0.46, 0.43, 0.31, 0.14, -0.11, -0.12, -0.12, -0.12, -0.1, 
				0
			}
		};

		public static Plot IABP_ABP_Nonpulsatile = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
				0, 0, 0, -0.01, -0.01, 0, 0.01, 0.13, 0.3, 0.49, 0.6, 0.66, 0.72, 0.77, 0.82, 
				0.86, 0.89, 0.92, 0.95, 0.97, 0.98, 0.99, 1, 0.99, 0.98, 0.96, 0.93, 0.9, 0.87, 0.85, 
				0.81, 0.81, 0.81, 0.81, 0.81, 0.81, 0.81, 0.81, 0.81, 0.8, 0.79, 0.78, 0.77, 0.76, 0.75, 
				0.74, 0.72, 0.69, 0.67, 0.65, 0.62, 0.57, 0.5, 0.43, 0.36, 0.25, -0.06, -0.09, -0.08, -0.07, 
				0
			}
		};

		public static Plot IABP_Balloon_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.06, 0.07, 0.4, 0.6, 0.8, 0.99, 0.99, 0.99, 0.99, 0.99, 0.99, 0.99, 0.99, 0.99, 0.99, 
				0.99, 0.99, 0.96, 0.92, 0.89, 0.85, 0.82, 0.79, 0.76, 0.73, 0.7, 0.68, 0.66, 0.64, 0.62, 
				0.6, 0.58, 0.56, 0.55, 0.54, 0.53, 0.52, 0.51, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 
				0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 
				0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.49, 0.49, 0.49, 0.49, 0.49, 
				0.49, 0.49, 0.49, 0.49, 0.49, 0.48, 0, 0, 0.01, 0.06
			}
		};

		public static Plot IAP_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.01, 0.02, 0.02, 0.03, 0.04, 0.05, 0.05, 0.06, 0.07, 0.07, 0.08, 0.08, 0.09, 0.1, 0.1, 
				0.11, 0.11, 0.12, 0.12, 0.13, 0.13, 0.14, 0.14, 0.15, 0.15, 0.15, 0.16, 0.16, 0.16, 0.17, 
				0.17, 0.17, 0.18, 0.18, 0.18, 0.18, 0.19, 0.19, 0.19, 0.19, 0.19, 0.19, 0.2, 0.2, 0.2, 
				0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.19, 0.19, 0.19, 
				0.19, 0.19, 0.19, 0.18, 0.18, 0.18, 0.18, 0.17, 0.17, 0.17, 0.16, 0.16, 0.16, 0.15, 0.15, 
				0.15, 0.14, 0.14, 0.13, 0.13, 0.12, 0.12, 0.11, 0.11, 0.1, 0.1, 0.09, 0.08, 0.08, 0.07, 
				0.07, 0.06, 0.05, 0.05, 0.04, 0.03, 0.02, 0.02, 0.01, 0, 0
			}
		};

		public static Plot ICP_HighCompliance = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.08, 0.16, 0.23, 0.3, 0.36, 0.41, 0.46, 0.51, 0.55, 0.59, 0.62, 0.65, 0.67, 0.68, 0.69, 
				0.7, 0.7, 0.7, 0.69, 0.69, 0.68, 0.67, 0.66, 0.65, 0.64, 0.63, 0.61, 0.6, 0.58, 0.56, 
				0.54, 0.52, 0.5, 0.51, 0.51, 0.52, 0.52, 0.53, 0.53, 0.53, 0.54, 0.54, 0.54, 0.54, 0.55, 
				0.55, 0.55, 0.55, 0.55, 0.55, 0.55, 0.55, 0.54, 0.54, 0.53, 0.52, 0.52, 0.51, 0.5, 0.48, 
				0.47, 0.46, 0.44, 0.43, 0.41, 0.4, 0.38, 0.35, 0.33, 0.32, 0.3, 0.28, 0.27, 0.25, 0.24, 
				0.23, 0.22, 0.22, 0.21, 0.21, 0.2, 0.2, 0.2, 0.2, 0.19, 0.19, 0.18, 0.17, 0.16, 0.15, 
				0.14, 0.13, 0.11, 0.1, 0.08, 0.06, 0.04, 0.02, 0
			}
		};

		public static Plot ICP_LowCompliance = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.08, 0.16, 0.23, 0.3, 0.36, 0.41, 0.46, 0.51, 0.55, 0.59, 0.62, 0.65, 0.67, 0.68, 0.69, 
				0.7, 0.7, 0.7, 0.69, 0.69, 0.68, 0.67, 0.66, 0.65, 0.64, 0.63, 0.61, 0.6, 0.58, 0.56, 
				0.54, 0.52, 0.5, 0.53, 0.57, 0.6, 0.63, 0.65, 0.68, 0.7, 0.72, 0.74, 0.75, 0.77, 0.78, 
				0.79, 0.79, 0.8, 0.8, 0.8, 0.8, 0.8, 0.79, 0.79, 0.79, 0.78, 0.78, 0.77, 0.76, 0.76, 
				0.75, 0.74, 0.73, 0.72, 0.71, 0.7, 0.69, 0.68, 0.67, 0.66, 0.65, 0.64, 0.63, 0.63, 0.62, 
				0.62, 0.61, 0.61, 0.6, 0.6, 0.6, 0.6, 0.6, 0.59, 0.58, 0.57, 0.55, 0.52, 0.49, 0.46, 
				0.43, 0.38, 0.34, 0.29, 0.23, 0.18, 0.11, 0.05, 0
			}
		};

		public static Plot PA_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.01, 0.13, 0.27, 0.49, 0.6, 0.66, 0.72, 0.78, 0.81, 0.83, 0.84, 0.84, 0.86, 0.87, 
				0.87, 0.86, 0.83, 0.83, 0.73, 0.7, 0.65, 0.6, 0.49, 0.48, 0.47, 0.46, 0.4, 0.28, 0.26, 
				0.24, 0.24, 0.24, 0.26, 0.28, 0.3, 0.3, 0.29, 0.26, 0.24, 0.21, 0.19, 0.16, 0.13, 0.11, 
				0.09, 0.07, 0.06, 0.06, 0.05, 0.07, 0.06, 0.06, 0.07, 0.05, 0.02, -0.01, -0.03, -0.01, 0.02, 
				0.05, 0.05, 0.02, 0.02, -0.01, -0.04, -0.06, -0.03, -0.04, 0, 0.04, 0.08, 0.06, 0.03, 0.02, 
				0
			}
		};

		public static Plot PCW_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0, 0, 0.01, 0.03, 0.05, 0.05, 0.07, 0.09, 0.11, 0.12, 0.13, 0.16, 0.18, 0.21, 
				0.22, 0.24, 0.26, 0.3, 0.35, 0.41, 0.44, 0.44, 0.44, 0.45, 0.45, 0.45, 0.43, 0.4, 0.36, 
				0.32, 0.28, 0.25, 0.22, 0.21, 0.19, 0.18, 0.17, 0.16, 0.16, 0.16, 0.17, 0.2, 0.21, 0.24, 
				0.27, 0.3, 0.34, 0.41, 0.48, 0.48, 0.48, 0.49, 0.49, 0.48, 0.5, 0.53, 0.56, 0.58, 0.58, 
				0.56, 0.52, 0.44, 0.37, 0.31, 0.25, 0.21, 0.17, 0.14, 0.13, 0.1, 0.08, 0.07, 0.04, 0.03, 
				0.01
			}
		};

		public static Plot RV_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.06, 0.08, 0.14, 0.21, 0.27, 0.37, 0.47, 0.57, 0.66, 0.75, 0.76, 0.73, 0.67, 0.62, 
				0.56, 0.56, 0.56, 0.59, 0.59, 0.6, 0.57, 0.56, 0.52, 0.49, 0.42, 0.34, 0.25, 0.17, 0.11, 
				0.04, -0.03, -0.12, -0.21, -0.3, -0.3, -0.25, -0.22, -0.2, -0.16, -0.16, -0.15, -0.13, -0.1, -0.09, 
				-0.1, -0.1, -0.12, -0.12, -0.12, -0.13, -0.14, -0.12, -0.1, -0.08, -0.05, -0.02, -0.02, -0.01, -0.01, 
				-0.01, -0.02, -0.01, 0, -0.01, 0, 0
			}
		};

		public static Plot SPO2_Rhythm = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0, 0.11, 0.21, 0.35, 0.48, 0.59, 0.69, 0.75, 0.8, 0.83, 0.85, 0.88, 0.89, 0.91, 0.91, 
				0.91, 0.9, 0.9, 0.89, 0.88, 0.87, 0.85, 0.82, 0.8, 0.77, 0.73, 0.71, 0.68, 0.65, 0.61, 
				0.56, 0.53, 0.52, 0.51, 0.52, 0.54, 0.56, 0.56, 0.53, 0.51, 0.48, 0.45, 0.44, 0.42, 0.4, 
				0.38, 0.38, 0.36, 0.35, 0.33, 0.32, 0.28, 0.25, 0.24, 0.22, 0.2, 0.18, 0.17, 0.14, 0.11, 
				0.09, 0.08, 0.07, 0.06, 0.05, 0.04, 0.03, 0.02, 0.01, 0.01, 0.01, 0.01, 0, 0, 0, 
				0
			}
		};

	}
}
