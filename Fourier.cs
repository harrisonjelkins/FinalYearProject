using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTransform
{
    public class Complex
    {
        public float real;
        public float imaginary;

        //Constructor for the complex mumber variable

        public Complex()
        {
            this.real = 0.0f;
            this.imaginary = 0.0f;
        }
        public Complex(float real, float imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public Complex(double real, double imaginary)
        {
            this.real = (float)real;
            this.imaginary = (float)imaginary;
        }

        //Display complex numbers in an appropriate form
        public string displayNumber(Complex number)
        {
            string data = real.ToString() + "" + imaginary.ToString() + "j";
            return data;
        }

        //convert Polar form to Rectangular Form
        public static Complex polar2rectangular(double r, double radians)
        {
            //double a = r * Math.Cos(radians);
            //double b = r * Math.Sin(radians);
            Complex result = new Complex((double)r * Math.Cos(radians), (double)r * Math.Sin(radians));
            return result;
        }

        //Basic Math Operators
        public static Complex operator +(Complex a, Complex b)
        {
            Complex data = new Complex(a.real + b.real, a.imaginary + b.imaginary);
            return data;
        }

        public static Complex operator -(Complex a, Complex b)
        {
            Complex data = new Complex(a.real - b.real, a.imaginary - b.imaginary);
            return data;
        }

        public static Complex operator *(Complex a, Complex b)
        {
            Complex data = new Complex((a.real * b.real) - (a.imaginary * b.imaginary),
                                       (a.real * b.imaginary) + (a.imaginary * b.real));
            return data;
        }

        //Get Polar Form data from Rectangular form
        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt(Math.Pow(real, 2) + Math.Pow(imaginary, 2));
                //return result;
            }
        }

        public float phase
        {
            get
            {
                return (float)Math.Atan(imaginary / real);
            }
        }
    }

    class Fourier
    {
        public static Complex[] FFT(Complex[] x)
        {
            int N = x.Length;
            int k;
            Complex[] X = new Complex[N];
            Complex[] d, D, e, E;

            //If only one value in the input Vector
            if (N == 1)
            {
                X[0] = x[0];
            }
            else
            {
                e = new Complex[N / 2];
                d = new Complex[N / 2];

                for (k = 0; k < N / 2; k++)
                {
                    e[k] = x[2 * k];
                    d[k] = x[2 * k + 1];
                }

                D = FFT(d);
                E = FFT(e);

                for (k = 0; k < N / 2; k++)
                {
                    Complex temp = Complex.polar2rectangular(1, -2 * Math.PI * k / N);
                    D[k] *= temp;
                }

                for (k = 0; k < N / 2; k++)
                {
                    X[k] = E[k] + D[k];
                    X[k + N / 2] = E[k] - D[k];
                }
            }
            return X;
    }

        public static Complex[] FloatFFT(float[] input)
        {
            int N = input.GetLength(0);
            Console.WriteLine("Filesize: " + N);
            const int nMax = 4096; //Max block size
            
            Complex[] x = new Complex[nMax];
            Complex[] X = new Complex[nMax];

            for(int i = 0; i < nMax; i++)
            {
                x[i] = new Complex();
                X[i] = new Complex();

                if (i < N)
                { 
                    x[i].real = i;
                }
                else
                {
                    x[i].real = 0;
                }
               
                //Console.WriteLine(X[i].real);
            }
            X = FFT(x);
            
            
            return X;
        }

        public static void FFTResults(Complex[] input, int sampleRate)
        {
            int N = input.Length;
            float resFrequency = sampleRate / N;
            float[,] result = new float[N,2];

            for(int i = 0; i < N; i++)
            {
                result[i, 0] = i * resFrequency * (180/(float)Math.PI);
                result[i, 1] = input[i].magnitude;
                Console.WriteLine("f: " + result[i, 0] + " A: " + result[i,1]);
            }

            return;
        }
    }
}
