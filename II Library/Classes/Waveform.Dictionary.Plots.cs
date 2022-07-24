using System;
using System.Collections.Generic;

namespace II.Waveform {
	public static partial class Dictionary {
		public static Plot ABP_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.12d, 0.24d, 0.34d, 0.43d, 0.51d, 0.58d, 0.65d, 0.7d, 0.74d, 0.77d, 0.79d, 0.8d, 0.8d, 0.8d, 
				0.79d, 0.79d, 0.78d, 0.78d, 0.77d, 0.76d, 0.75d, 0.74d, 0.72d, 0.71d, 0.69d, 0.65d, 0.61d, 0.57d, 0.54d, 
				0.51d, 0.48d, 0.46d, 0.44d, 0.42d, 0.41d, 0.4d, 0.4d, 0.4d, 0.39d, 0.38d, 0.37d, 0.35d, 0.33d, 0.31d, 
				0.28d, 0.24d, 0.21d, 0.17d, 0.12d, 0.1d, 0.1d, 0.1d, 0.09d, 0.09d, 0.08d, 0.08d, 0.08d, 0.07d, 0.07d, 
				0.06d, 0.06d, 0.06d, 0.05d, 0.05d, 0.04d, 0.04d, 0.04d, 0.03d, 0.03d, 0.02d, 0.02d, 0.02d, 0.01d, 0.01d, 
				0d
			}
		};

		public static Plot CVP_Atrioventricular = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.01d, 0.01d, 0.02d, 0.02d, 0.06d, 0.11d, 0.14d, 0.17d, 0.2d, 0.23d, 0.25d, 0.28d, 0.29d, 0.32d, 0.34d, 
				0.36d, 0.38d, 0.4d, 0.42d, 0.44d, 0.45d, 0.46d, 0.47d, 0.47d, 0.48d, 0.49d, 0.49d, 0.49d, 0.48d, 0.48d, 
				0.48d, 0.47d, 0.46d, 0.45d, 0.44d, 0.43d, 0.43d, 0.41d, 0.41d, 0.4d, 0.39d, 0.39d, 0.38d, 0.38d, 0.39d, 
				0.42d, 0.46d, 0.48d, 0.51d, 0.54d, 0.56d, 0.58d, 0.61d, 0.63d, 0.65d, 0.68d, 0.71d, 0.74d, 0.77d, 0.79d, 
				0.8d, 0.8d, 0.77d, 0.73d, 0.7d, 0.67d, 0.65d, 0.61d, 0.58d, 0.52d, 0.51d, 0.5d, 0.49d, 0.48d, 0.47d, 
				0.46d, 0.45d, 0.42d, 0.4d, 0.37d, 0.33d, 0.3d, 0.24d, 0.19d, 0.16d, 0.11d, 0.07d, 0.04d, 0.01d, -0.03d, 
				-0.04d, -0.05d, -0.05d, -0.05d, -0.01d
			}
		};

		public static Plot CVP_Ventricular = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				-0.01d, -0.01d, -0.01d, -0.01d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 
				0d, 0d, 0d, 0d, -0.01d, -0.01d, -0.01d, -0.01d, -0.01d, -0.01d, -0.01d, -0.01d, -0.01d, 0d, 0d, 
				0d, 0d, 0d, 0d, 0d, 0d, 0d, 0.01d, 0.03d, 0.06d, 0.14d, 0.21d, 0.26d, 0.29d, 0.37d, 
				0.41d, 0.46d, 0.5d, 0.54d, 0.56d, 0.58d, 0.6d, 0.63d, 0.65d, 0.68d, 0.7d, 0.73d, 0.76d, 0.79d, 0.81d, 
				0.81d, 0.78d, 0.76d, 0.72d, 0.7d, 0.66d, 0.63d, 0.6d, 0.58d, 0.5d, 0.49d, 0.48d, 0.48d, 0.46d, 0.46d, 
				0.44d, 0.42d, 0.38d, 0.35d, 0.32d, 0.27d, 0.22d, 0.18d, 0.13d, 0.08d, 0.03d, 0d, -0.01d, -0.02d, -0.02d, 
				-0.03d, -0.03d, -0.03d, -0.02d, 0d
			}
		};

		public static Plot ECG_Complex_Idioventricular = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.26d, 0.52d, 0.84d, 0.81d, 0.78d, 0.61d, 0.02d, -0.24d, -0.33d, -0.45d, -0.45d, -0.41d, -0.38d, -0.34d, 
				-0.31d, -0.31d, -0.33d, -0.34d, -0.38d, -0.38d, -0.37d, -0.31d, -0.25d, -0.11d, 0.01d, 0.06d, 0.09d, 0.11d, 0.13d, 
				0.12d, 0.11d, 0.1d, 0.09d, 0.09d, 0.08d, 0.07d, 0.07d, 0.05d, 0.05d, 0.05d, 0.05d, 0.05d, 0.04d, 0.04d, 
				0.03d, 0.03d, 0.03d, 0.03d, 0.02d, 0.02d, 0.02d, 0.01d, 0.01d, 0.01d, 0d, 0d, 0d, 0d, 0d, 
				0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 
				0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 
				0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d
			}
		};

		public static Plot ECG_Complex_VT = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				-0.02d, -0.03d, -0.04d, -0.05d, -0.06d, -0.07d, -0.08d, -0.09d, -0.09d, -0.1d, -0.1d, -0.1d, -0.1d, -0.1d, -0.11d, 
				-0.11d, -0.12d, -0.12d, -0.13d, -0.14d, -0.15d, -0.16d, -0.18d, -0.19d, -0.2d, -0.32d, -0.44d, -0.54d, -0.63d, -0.71d, 
				-0.78d, -0.85d, -0.9d, -0.94d, -0.97d, -0.99d, -1d, -1d, -0.98d, -0.96d, -0.93d, -0.89d, -0.84d, -0.78d, -0.71d, 
				-0.64d, -0.55d, -0.46d, -0.35d, -0.3d, -0.25d, -0.19d, -0.14d, -0.09d, -0.05d, 0d, 0.04d, 0.08d, 0.11d, 0.15d, 
				0.18d, 0.21d, 0.24d, 0.26d, 0.29d, 0.31d, 0.33d, 0.35d, 0.36d, 0.37d, 0.38d, 0.39d, 0.4d, 0.4d, 0.4d, 
				0.4d, 0.4d, 0.4d, 0.39d, 0.39d, 0.38d, 0.38d, 0.37d, 0.36d, 0.35d, 0.34d, 0.33d, 0.32d, 0.31d, 0.29d, 
				0.28d, 0.26d, 0.24d, 0.23d, 0.21d, 0.19d, 0.17d, 0.15d, 0.12d, 0.1d, 0.1d
			}
		};

		public static Plot ECG_CPR_Artifact = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, -0.03d, -0.04d, -0.02d, 0d, 0.01d, 0.04d, 0.06d, 0.06d, 0.04d, -0.02d, -0.08d, -0.14d, -0.13d, -0.14d, 
				-0.16d, -0.16d, -0.18d, -0.18d, -0.18d, -0.15d, -0.11d, -0.07d, -0.02d, 0.01d, 0.04d, 0.06d, 0.07d, 0.09d, 0.1d, 
				0.11d, 0.16d, 0.18d, 0.19d, 0.2d, 0.23d, 0.23d, 0.53d, 0.68d, 0.77d, 0.86d, 0.95d, 0.99d, 1d, 1d, 
				1d, 1d, 1d, 0.99d, 1d, 0.99d, 0.97d, 0.93d, 0.87d, 0.82d, 0.7d, 0.65d, 0.61d, 0.58d, 0.55d, 
				0.52d, 0.46d, 0.32d, 0.23d, 0.16d, 0.07d, 0.01d, -0.02d, 0d, -0.02d, -0.03d, 0.02d, 0.03d, 0.04d, 0.02d, 
				0d
			}
		};

		public static Plot ECG_Defibrillation = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 1d, 1d, 0.95d, 0.9d, 0.85d, 0.8d, 0.75d, 0.7d, 0.65d, 0.6d, 0.55d, 0.5d, 0.5d, -0.5d, 
				-0.5d, -0.44d, -0.38d, -0.31d, -0.25d, -0.19d, -0.12d, -0.06d, 0d
			}
		};

		public static Plot ECG_Pacemaker = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.1d, 0.2d, 0.2d, 0.1d, 0d, 0d, 0d, 0d
			}
		};

		public static Plot EFM_Contraction = new Plot () {
			DrawResolution = 100,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.01d, 0.01d, 0.01d, 0.02d, 0.02d, 0.02d, 0.02d, 0.03d, 0.03d, 0.03d, 0.04d, 0.04d, 0.04d, 0.05d, 
				0.05d, 0.05d, 0.05d, 0.06d, 0.06d, 0.06d, 0.07d, 0.07d, 0.07d, 0.08d, 0.09d, 0.09d, 0.1d, 0.1d, 0.1d, 
				0.1d, 0.11d, 0.12d, 0.12d, 0.13d, 0.14d, 0.15d, 0.16d, 0.17d, 0.18d, 0.19d, 0.2d, 0.21d, 0.22d, 0.23d, 
				0.23d, 0.24d, 0.25d, 0.26d, 0.26d, 0.27d, 0.28d, 0.29d, 0.3d, 0.31d, 0.32d, 0.32d, 0.33d, 0.34d, 0.35d, 
				0.36d, 0.37d, 0.38d, 0.39d, 0.39d, 0.4d, 0.41d, 0.42d, 0.43d, 0.44d, 0.45d, 0.46d, 0.47d, 0.48d, 0.48d, 
				0.49d, 0.5d, 0.51d, 0.52d, 0.53d, 0.53d, 0.54d, 0.55d, 0.55d, 0.56d, 0.56d, 0.57d, 0.58d, 0.58d, 0.59d, 
				0.59d, 0.6d, 0.6d, 0.61d, 0.61d, 0.62d, 0.63d, 0.63d, 0.64d, 0.64d, 0.65d, 0.66d, 0.66d, 0.67d, 0.68d, 
				0.69d, 0.69d, 0.7d, 0.71d, 0.71d, 0.72d, 0.73d, 0.73d, 0.74d, 0.75d, 0.75d, 0.76d, 0.76d, 0.77d, 0.77d, 
				0.78d, 0.79d, 0.79d, 0.8d, 0.8d, 0.81d, 0.82d, 0.82d, 0.83d, 0.83d, 0.84d, 0.84d, 0.85d, 0.85d, 0.86d, 
				0.86d, 0.86d, 0.87d, 0.87d, 0.87d, 0.87d, 0.88d, 0.88d, 0.88d, 0.88d, 0.89d, 0.89d, 0.89d, 0.9d, 0.9d, 
				0.91d, 0.91d, 0.91d, 0.92d, 0.92d, 0.93d, 0.93d, 0.93d, 0.94d, 0.94d, 0.94d, 0.94d, 0.95d, 0.95d, 0.95d, 
				0.95d, 0.96d, 0.97d, 0.97d, 0.97d, 0.97d, 0.98d, 0.98d, 0.98d, 0.99d, 0.99d, 0.99d, 0.99d, 0.99d, 0.99d, 
				1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 0.99d, 
				0.99d, 0.99d, 0.99d, 0.99d, 0.99d, 0.99d, 0.98d, 0.98d, 0.98d, 0.98d, 0.97d, 0.97d, 0.97d, 0.96d, 0.96d, 
				0.95d, 0.95d, 0.95d, 0.94d, 0.93d, 0.92d, 0.91d, 0.9d, 0.89d, 0.89d, 0.88d, 0.87d, 0.86d, 0.85d, 0.83d, 
				0.82d, 0.79d, 0.78d, 0.77d, 0.75d, 0.74d, 0.73d, 0.72d, 0.71d, 0.69d, 0.68d, 0.66d, 0.63d, 0.61d, 0.56d, 
				0.55d, 0.52d, 0.49d, 0.48d, 0.47d, 0.43d, 0.41d, 0.37d, 0.33d, 0.31d, 0.3d, 0.28d, 0.27d, 0.26d, 0.25d, 
				0.23d, 0.22d, 0.21d, 0.2d, 0.19d, 0.18d, 0.17d, 0.15d, 0.14d, 0.13d, 0.12d, 0.11d, 0.1d, 0.09d, 0.08d, 
				0.08d, 0.07d, 0.06d, 0.06d, 0.05d, 0.05d, 0.04d, 0.04d, 0.04d, 0.03d, 0.03d, 0.03d, 0.02d, 0.02d, 0.02d, 
				0.02d, 0.01d, 0.01d, 0.01d, 0.01d, 0.01d, 0.01d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d
			}
		};

		public static Plot EFM_Variability = new Plot () {
			DrawResolution = 100,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, -0.01d, -0.01d, -0.01d, -0.02d, -0.03d, 
				-0.03d, -0.04d, -0.05d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.06d, 
				-0.06d, -0.06d, -0.06d, -0.06d, -0.06d, -0.05d, -0.05d, -0.04d, -0.04d, -0.04d, -0.03d, -0.03d, -0.03d, -0.02d, -0.02d, 
				-0.01d, 0d, 0.01d, 0.02d, 0.03d, 0.04d, 0.04d, 0.05d, 0.05d, 0.05d, 0.05d, 0.05d, 0.05d, 0.05d, 0.05d, 
				0.04d, 0.03d, 0.03d, 0.02d, 0.01d, 0.01d, 0d, 0d, 0d, -0.01d, -0.02d, -0.02d, -0.02d, -0.03d, -0.03d, 
				-0.04d, -0.04d, -0.04d, -0.04d, -0.04d, -0.04d, -0.04d, -0.04d, -0.03d, -0.03d, -0.03d, -0.02d, -0.02d, -0.02d, -0.01d, 
				-0.01d, -0.01d, -0.01d, -0.01d, -0.01d, 0d, 0d, 0d, 0d, 0d
			}
		};

		public static Plot ETCO2_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.11d, 0.22d, 0.31d, 0.38d, 0.45d, 0.5d, 0.55d, 0.59d, 0.6d, 0.62d, 0.63d, 0.64d, 0.64d, 0.64d, 0.65d, 
				0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 
				0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.65d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 
				0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 
				0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 0.66d, 
				0.66d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 
				0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 
				0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.67d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 
				0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 
				0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.68d, 0.69d, 
				0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 
				0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 
				0.69d, 0.69d, 0.69d, 0.69d, 0.69d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 
				0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 0.7d, 
				0.7d, 0.7d, 0.7d, 0.69d, 0.67d, 0.64d, 0.59d, 0.52d, 0.45d, 0.36d, 0.25d, 0.13d, 0d, 0d
			}
		};

		public static Plot IABP_ABP_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.12d, 0.24d, 0.34d, 0.43d, 0.51d, 0.58d, 0.65d, 0.7d, 0.74d, 0.77d, 0.79d, 0.8d, 0.8d, 0.79d, 
				0.79d, 0.78d, 0.77d, 0.75d, 0.74d, 0.72d, 0.7d, 0.67d, 0.65d, 0.62d, 0.6d, 0.66d, 0.72d, 0.77d, 0.82d, 
				0.86d, 0.89d, 0.92d, 0.95d, 0.97d, 0.98d, 0.99d, 1d, 0.99d, 0.98d, 0.95d, 0.93d, 0.9d, 0.87d, 0.85d, 
				0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.8d, 0.8d, 0.8d, 0.79d, 0.78d, 0.77d, 0.76d, 0.75d, 
				0.74d, 0.72d, 0.69d, 0.67d, 0.65d, 0.62d, 0.57d, 0.5d, 0.43d, 0.36d, 0.25d, -0.06d, -0.09d, -0.08d, -0.07d, 
				0d
			}
		};

		public static Plot IABP_ABP_Ectopic = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.06d, 0.12d, 0.17d, 0.22d, 0.26d, 0.29d, 0.32d, 0.35d, 0.37d, 0.38d, 0.39d, 0.4d, 0.4d, 0.4d, 
				0.39d, 0.39d, 0.38d, 0.38d, 0.37d, 0.36d, 0.35d, 0.34d, 0.32d, 0.31d, 0.3d, 0.38d, 0.45d, 0.51d, 0.57d, 
				0.62d, 0.66d, 0.7d, 0.74d, 0.76d, 0.78d, 0.79d, 0.8d, 0.79d, 0.78d, 0.75d, 0.72d, 0.67d, 0.62d, 0.57d, 
				0.56d, 0.55d, 0.54d, 0.54d, 0.54d, 0.54d, 0.54d, 0.54d, 0.54d, 0.54d, 0.54d, 0.54d, 0.54d, 0.53d, 0.53d, 
				0.53d, 0.52d, 0.51d, 0.5d, 0.49d, 0.48d, 0.46d, 0.43d, 0.31d, 0.14d, -0.11d, -0.12d, -0.12d, -0.12d, -0.1d, 
				0d
			}
		};

		public static Plot IABP_ABP_Nonpulsatile = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 
				0d, 0d, 0d, -0.01d, -0.01d, 0d, 0.01d, 0.13d, 0.3d, 0.49d, 0.6d, 0.66d, 0.72d, 0.77d, 0.82d, 
				0.86d, 0.89d, 0.92d, 0.95d, 0.97d, 0.98d, 0.99d, 1d, 0.99d, 0.98d, 0.96d, 0.93d, 0.9d, 0.87d, 0.85d, 
				0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.81d, 0.8d, 0.79d, 0.78d, 0.77d, 0.76d, 0.75d, 
				0.74d, 0.72d, 0.69d, 0.67d, 0.65d, 0.62d, 0.57d, 0.5d, 0.43d, 0.36d, 0.25d, -0.06d, -0.09d, -0.08d, -0.07d, 
				0d
			}
		};

		public static Plot IABP_Balloon_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.2d, 0.4d, 0.6d, 0.8d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 
				1d, 1d, 0.96d, 0.92d, 0.89d, 0.85d, 0.82d, 0.79d, 0.76d, 0.73d, 0.7d, 0.68d, 0.66d, 0.64d, 0.62d, 
				0.6d, 0.58d, 0.56d, 0.55d, 0.54d, 0.53d, 0.52d, 0.51d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 
				0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 
				0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.49d, 0.49d, 0.49d, 0.49d, 0.49d, 
				0.49d, 0.49d, 0.49d, 0.49d, 0.49d, 0.48d, -0.13d, -0.16d, -0.14d, 0d
			}
		};

		public static Plot IAP_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.01d, 0.02d, 0.02d, 0.03d, 0.04d, 0.05d, 0.05d, 0.06d, 0.07d, 0.07d, 0.08d, 0.08d, 0.09d, 0.1d, 0.1d, 
				0.11d, 0.11d, 0.12d, 0.12d, 0.13d, 0.13d, 0.14d, 0.14d, 0.15d, 0.15d, 0.15d, 0.16d, 0.16d, 0.16d, 0.17d, 
				0.17d, 0.17d, 0.18d, 0.18d, 0.18d, 0.18d, 0.19d, 0.19d, 0.19d, 0.19d, 0.19d, 0.19d, 0.2d, 0.2d, 0.2d, 
				0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.2d, 0.19d, 0.19d, 0.19d, 
				0.19d, 0.19d, 0.19d, 0.18d, 0.18d, 0.18d, 0.18d, 0.17d, 0.17d, 0.17d, 0.16d, 0.16d, 0.16d, 0.15d, 0.15d, 
				0.15d, 0.14d, 0.14d, 0.13d, 0.13d, 0.12d, 0.12d, 0.11d, 0.11d, 0.1d, 0.1d, 0.09d, 0.08d, 0.08d, 0.07d, 
				0.07d, 0.06d, 0.05d, 0.05d, 0.04d, 0.03d, 0.02d, 0.02d, 0.01d, 0d, 0d
			}
		};

		public static Plot ICP_HighCompliance = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.08d, 0.16d, 0.23d, 0.3d, 0.36d, 0.41d, 0.46d, 0.51d, 0.55d, 0.59d, 0.62d, 0.65d, 0.67d, 0.68d, 0.69d, 
				0.7d, 0.7d, 0.7d, 0.69d, 0.69d, 0.68d, 0.67d, 0.66d, 0.65d, 0.64d, 0.63d, 0.61d, 0.6d, 0.58d, 0.56d, 
				0.54d, 0.52d, 0.5d, 0.51d, 0.51d, 0.52d, 0.52d, 0.53d, 0.53d, 0.53d, 0.54d, 0.54d, 0.54d, 0.54d, 0.55d, 
				0.55d, 0.55d, 0.55d, 0.55d, 0.55d, 0.55d, 0.55d, 0.54d, 0.54d, 0.53d, 0.52d, 0.52d, 0.51d, 0.5d, 0.48d, 
				0.47d, 0.46d, 0.44d, 0.43d, 0.41d, 0.4d, 0.38d, 0.35d, 0.33d, 0.32d, 0.3d, 0.28d, 0.27d, 0.25d, 0.24d, 
				0.23d, 0.22d, 0.22d, 0.21d, 0.21d, 0.2d, 0.2d, 0.2d, 0.2d, 0.19d, 0.19d, 0.18d, 0.17d, 0.16d, 0.15d, 
				0.14d, 0.13d, 0.11d, 0.1d, 0.08d, 0.06d, 0.04d, 0.02d, 0d
			}
		};

		public static Plot ICP_LowCompliance = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0.08d, 0.16d, 0.23d, 0.3d, 0.36d, 0.41d, 0.46d, 0.51d, 0.55d, 0.59d, 0.62d, 0.65d, 0.67d, 0.68d, 0.69d, 
				0.7d, 0.7d, 0.7d, 0.69d, 0.69d, 0.68d, 0.67d, 0.66d, 0.65d, 0.64d, 0.63d, 0.61d, 0.6d, 0.58d, 0.56d, 
				0.54d, 0.52d, 0.5d, 0.53d, 0.57d, 0.6d, 0.63d, 0.65d, 0.68d, 0.7d, 0.72d, 0.74d, 0.75d, 0.77d, 0.78d, 
				0.79d, 0.79d, 0.8d, 0.8d, 0.8d, 0.8d, 0.8d, 0.79d, 0.79d, 0.79d, 0.78d, 0.78d, 0.77d, 0.76d, 0.76d, 
				0.75d, 0.74d, 0.73d, 0.72d, 0.71d, 0.7d, 0.69d, 0.68d, 0.67d, 0.66d, 0.65d, 0.64d, 0.63d, 0.63d, 0.62d, 
				0.62d, 0.61d, 0.61d, 0.6d, 0.6d, 0.6d, 0.6d, 0.6d, 0.59d, 0.58d, 0.57d, 0.55d, 0.52d, 0.49d, 0.46d, 
				0.43d, 0.38d, 0.34d, 0.29d, 0.23d, 0.18d, 0.11d, 0.05d, 0d
			}
		};

		public static Plot PA_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.01d, 0.13d, 0.27d, 0.49d, 0.6d, 0.66d, 0.72d, 0.78d, 0.81d, 0.83d, 0.84d, 0.84d, 0.86d, 0.87d, 
				0.87d, 0.86d, 0.83d, 0.83d, 0.73d, 0.7d, 0.65d, 0.6d, 0.49d, 0.48d, 0.47d, 0.46d, 0.4d, 0.28d, 0.26d, 
				0.24d, 0.24d, 0.24d, 0.26d, 0.28d, 0.3d, 0.3d, 0.29d, 0.26d, 0.24d, 0.21d, 0.19d, 0.16d, 0.13d, 0.11d, 
				0.09d, 0.07d, 0.06d, 0.06d, 0.05d, 0.07d, 0.06d, 0.06d, 0.07d, 0.05d, 0.02d, -0.01d, -0.03d, -0.01d, 0.02d, 
				0.05d, 0.05d, 0.02d, 0.02d, -0.01d, -0.04d, -0.06d, -0.03d, -0.04d, 0d, 0.04d, 0.08d, 0.06d, 0.03d, 0.02d, 
				0d
			}
		};

		public static Plot PCW_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0d, 0d, 0.01d, 0.03d, 0.05d, 0.05d, 0.07d, 0.09d, 0.11d, 0.12d, 0.13d, 0.16d, 0.18d, 0.21d, 
				0.22d, 0.24d, 0.26d, 0.3d, 0.35d, 0.41d, 0.44d, 0.44d, 0.44d, 0.45d, 0.45d, 0.45d, 0.43d, 0.4d, 0.36d, 
				0.32d, 0.28d, 0.25d, 0.22d, 0.21d, 0.19d, 0.18d, 0.17d, 0.16d, 0.16d, 0.16d, 0.17d, 0.2d, 0.21d, 0.24d, 
				0.27d, 0.3d, 0.34d, 0.41d, 0.48d, 0.48d, 0.48d, 0.49d, 0.49d, 0.48d, 0.5d, 0.53d, 0.56d, 0.58d, 0.58d, 
				0.56d, 0.52d, 0.44d, 0.37d, 0.31d, 0.25d, 0.21d, 0.17d, 0.14d, 0.13d, 0.1d, 0.08d, 0.07d, 0.04d, 0.03d, 
				0.01d
			}
		};

		public static Plot RV_Default = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.06d, 0.08d, 0.14d, 0.21d, 0.27d, 0.37d, 0.47d, 0.57d, 0.66d, 0.75d, 0.76d, 0.73d, 0.67d, 0.62d, 
				0.56d, 0.56d, 0.56d, 0.59d, 0.59d, 0.6d, 0.57d, 0.56d, 0.52d, 0.49d, 0.42d, 0.34d, 0.25d, 0.17d, 0.11d, 
				0.04d, -0.03d, -0.12d, -0.21d, -0.3d, -0.3d, -0.25d, -0.22d, -0.2d, -0.16d, -0.16d, -0.15d, -0.13d, -0.1d, -0.09d, 
				-0.1d, -0.1d, -0.12d, -0.12d, -0.12d, -0.13d, -0.14d, -0.12d, -0.1d, -0.08d, -0.05d, -0.02d, -0.02d, -0.01d, -0.01d, 
				-0.01d, -0.02d, -0.01d, 0d, -0.01d, 0d, 0d
			}
		};

		public static Plot SPO2_Rhythm = new Plot () {
			DrawResolution = 10,
			IndexOffset = 0,
			Vertices = new double[] {
				0d, 0.11d, 0.21d, 0.35d, 0.48d, 0.59d, 0.69d, 0.75d, 0.8d, 0.83d, 0.85d, 0.88d, 0.89d, 0.91d, 0.91d, 
				0.91d, 0.9d, 0.9d, 0.89d, 0.88d, 0.87d, 0.85d, 0.82d, 0.8d, 0.77d, 0.73d, 0.71d, 0.68d, 0.65d, 0.61d, 
				0.56d, 0.53d, 0.52d, 0.51d, 0.52d, 0.54d, 0.56d, 0.56d, 0.53d, 0.51d, 0.48d, 0.45d, 0.44d, 0.42d, 0.4d, 
				0.38d, 0.38d, 0.36d, 0.35d, 0.33d, 0.32d, 0.28d, 0.25d, 0.24d, 0.22d, 0.2d, 0.18d, 0.17d, 0.14d, 0.11d, 
				0.09d, 0.08d, 0.07d, 0.06d, 0.05d, 0.04d, 0.03d, 0.02d, 0.01d, 0.01d, 0.01d, 0.01d, 0d, 0d, 0d, 
				0d
			}
		};

	}
}
