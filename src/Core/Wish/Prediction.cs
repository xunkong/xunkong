namespace Xunkong.Core.Wish
{
    public class Prediction
    {
        private static readonly double[] InitCharacterDensity;

        private static readonly double[] InitWeaponDensity;

        private static readonly double[] InitCharacterDensityWithUp;

        private static readonly double[] InitWeaponDensityWithUp;

        private static readonly double[] InitSpecifiedWeaponDensity;

        static Prediction()
        {
            InitCharacterDensity = GetInitCharacterDensity();
            InitWeaponDensity = GetInitWeaponDensity();
            InitCharacterDensityWithUp = GetInitCharacterDensityWithUp();
            InitWeaponDensityWithUp = GetInitWeaponDensityWithUp();
            InitSpecifiedWeaponDensity = GetInitSpecifiedWeaponDensity();
        }

        public static double GetCharacterProbability(int n)
        {
            if (n < 1 || n > 90)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (n < 74)
            {
                return 0.006;
            }
            else if (n < 90)
            {
                return 0.06 * (n - 73) + 0.006;
            }
            else
            {
                return 1;
            }
        }

        public static double GetWeaponProbability(int n)
        {
            if (n < 1 || n > 80)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (n < 63)
            {
                return 0.007;
            }
            else if (n < 74)
            {
                return 0.07 * (n - 62) + 0.007;
            }
            else if (n < 80)
            {
                return 0.035 * (n - 73) + 0.777;
            }
            else
            {
                return 1;
            }
        }


        public static (double[] density, double[] distribution) GetCharacterDensityAndDistribution(int star5Count)
        {
            if (star5Count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            // 临时概率密度
            var tmp = new double[90 * star5Count];
            // 概率密度
            var den = new double[90 * star5Count];
            // 分布函数
            var dis = new double[90 * star5Count];

            InitCharacterDensity.CopyTo(den, 0);
            InitCharacterDensity.CopyTo(tmp, 0);

            for (int n = 2; n <= star5Count; n++)
            {
                Array.Clear(den, 0, den.Length);
                Parallel.For(n, 90 * n + 1, i =>
                {
                    for (int j = 1; j <= 90 && i - j > 0; j++)
                    {
                        den[i - 1] += tmp[i - j - 1] * InitCharacterDensity[j - 1];
                    }
                });
                den.CopyTo(tmp, 0);
            }

            dis[0] = den[0];
            for (int i = 1; i < den.Length; i++)
            {
                dis[i] = dis[i - 1] + den[i];
            }
            return (den, dis);
        }


        public static (double[] density, double[] distribution) GetWeaponDensityAndDistribution(int star5Count)
        {
            if (star5Count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var tmp = new double[80 * star5Count];
            var den = new double[80 * star5Count];
            var dis = new double[80 * star5Count];


            InitWeaponDensity.CopyTo(den, 0);
            InitWeaponDensity.CopyTo(tmp, 0);

            for (int n = 2; n <= star5Count; n++)
            {
                Array.Clear(den, 0, den.Length);
                Parallel.For(n, 80 * n + 1, i =>
                {
                    for (int j = 1; j <= 80 && i - j > 0; j++)
                    {
                        den[i - 1] += tmp[i - j - 1] * InitWeaponDensity[j - 1];
                    }
                });
                den.CopyTo(tmp, 0);
            }

            dis[0] = den[0];
            for (int i = 1; i < den.Length; i++)
            {
                dis[i] = dis[i - 1] + den[i];
            }
            return (den, dis);
        }


        public static (double[] density, double[] distribution) GetCharacterDensityAndDistributionWithUp(int star5Count)
        {
            if (star5Count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var tmp = new double[180 * star5Count];
            var den = new double[180 * star5Count];
            var dis = new double[180 * star5Count];


            InitCharacterDensityWithUp.CopyTo(den, 0);
            InitCharacterDensityWithUp.CopyTo(tmp, 0);

            for (int n = 2; n <= star5Count; n++)
            {
                Array.Clear(den, 0, den.Length);
                Parallel.For(n, 180 * n + 1, i =>
                {
                    for (int j = 1; j <= 180 && i - j > 0; j++)
                    {
                        den[i - 1] += tmp[i - j - 1] * InitCharacterDensityWithUp[j - 1];
                    }
                });
                den.CopyTo(tmp, 0);
            }

            dis[0] = den[0];
            for (int i = 1; i < den.Length; i++)
            {
                dis[i] = dis[i - 1] + den[i];
            }
            return (den, dis);
        }


        public static (double[] density, double[] distribution) GetWeaponDensityAndDistributionWithUp(int star5Count)
        {
            if (star5Count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var tmp = new double[160 * star5Count];
            var den = new double[160 * star5Count];
            var dis = new double[160 * star5Count];


            InitWeaponDensityWithUp.CopyTo(den, 0);
            InitWeaponDensityWithUp.CopyTo(tmp, 0);

            for (int n = 2; n <= star5Count; n++)
            {
                Array.Clear(den, 0, den.Length);
                Parallel.For(n, 160 * n + 1, i =>
                {
                    for (int j = 1; j <= 160 && i - j > 0; j++)
                    {
                        den[i - 1] += tmp[i - j - 1] * InitWeaponDensityWithUp[j - 1];
                    }
                });
                den.CopyTo(tmp, 0);
            }

            dis[0] = den[0];
            for (int i = 1; i < den.Length; i++)
            {
                dis[i] = dis[i - 1] + den[i];
            }
            return (den, dis);
        }



        public static (double[] density, double[] distribution) GetSpecifiedWeaponDensityAndDistribution(int star5Count)
        {
            if (star5Count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var tmp = new double[240 * star5Count];
            var den = new double[240 * star5Count];
            var dis = new double[240 * star5Count];


            InitSpecifiedWeaponDensity.CopyTo(den, 0);
            InitSpecifiedWeaponDensity.CopyTo(tmp, 0);

            for (int n = 2; n <= star5Count; n++)
            {
                Array.Clear(den, 0, den.Length);
                Parallel.For(n, 240 * n + 1, i =>
                {
                    for (int j = 1; j <= 240 && i - j > 0; j++)
                    {
                        den[i - 1] += tmp[i - j - 1] * InitSpecifiedWeaponDensity[j - 1];
                    }
                });
                den.CopyTo(tmp, 0);
            }

            dis[0] = den[0];
            for (int i = 1; i < den.Length; i++)
            {
                dis[i] = dis[i - 1] + den[i];
            }
            return (den, dis);
        }


        private static double[] GetInitCharacterDensity()
        {
            var result = new double[90];
            result[0] = 0.006;
            for (int i = 1; i < result.Length; i++)
            {
                result[i] = result[i - 1] / GetCharacterProbability(i) * (1 - GetCharacterProbability(i)) * GetCharacterProbability(i + 1);
            }
            return result;
        }

        private static double[] GetInitWeaponDensity()
        {
            var result = new double[80];
            result[0] = 0.007;
            for (int i = 1; i < result.Length; i++)
            {
                result[i] = result[i - 1] / GetWeaponProbability(i) * (1 - GetWeaponProbability(i)) * GetWeaponProbability(i + 1);
            }
            return result;
        }

        private static double[] GetInitCharacterDensityWithUp()
        {
            var tmp = new double[180];
            InitCharacterDensity.CopyTo(tmp, 0);
            var result = new double[180];
            for (int i = 1; i <= result.Length; i++)
            {
                result[i - 1] += 0.5 * tmp[i - 1];
                for (int j = 1; j <= 90 && i - j > 0; j++)
                {
                    result[i - 1] += 0.5 * tmp[i - j - 1] * InitCharacterDensity[j - 1];
                }
            }
            return result;
        }

        private static double[] GetInitWeaponDensityWithUp()
        {
            var tmp = new double[160];
            InitCharacterDensity.CopyTo(tmp, 0);
            var result = new double[160];
            for (int i = 1; i <= result.Length; i++)
            {
                result[i - 1] += 0.75 * tmp[i - 1];
                for (int j = 1; j <= 80 && i - j > 0; j++)
                {
                    result[i - 1] += 0.25 * tmp[i - j - 1] * InitWeaponDensity[j - 1];
                }
            }
            return result;
        }

        private static double[] GetInitSpecifiedWeaponDensity()
        {
            var dis = new double[240];
            var res = new double[240];
            var tmp1 = new double[240];
            var tmp2 = new double[240];
            var tmp3 = new double[240];


            for (int i = 0; i < InitWeaponDensity.Length; i++)
            {
                res[i] += 0.375 * InitWeaponDensity[i];
            }
            InitWeaponDensity.CopyTo(tmp1, 0);
            for (int i = 2; i <= 160; i++)
            {
                for (int j = 1; j <= 80 && i - j > 0; j++)
                {
                    dis[i - 1] += 0.375 * tmp1[i - j - 1] * 0.375 * InitWeaponDensity[j - 1];
                    dis[i - 1] += 0.25 * tmp1[i - j - 1] * 0.5 * InitWeaponDensity[j - 1];
                }
            }
            for (int i = 0; i < dis.Length; i++)
            {
                res[i] += dis[i];
            }
            Array.Clear(dis, 0, dis.Length);

            for (int i = 2; i <= 160; i++)
            {
                for (int j = 1; j <= 80 && i - j > 0; j++)
                {
                    tmp2[i - 1] += 0.375 * tmp1[i - j - 1] * InitWeaponDensity[j - 1];
                    tmp3[i - 1] += 0.25 * tmp1[i - j - 1] * InitWeaponDensity[j - 1];
                }
            }
            for (int i = 3; i <= 240; i++)
            {
                for (int j = 1; j <= 80 && i - j > 0; j++)
                {
                    dis[i - 1] += 0.375 * tmp2[i - j - 1] * InitWeaponDensity[j - 1];
                    dis[i - 1] += 0.25 * tmp2[i - j - 1] * InitWeaponDensity[j - 1];
                    dis[i - 1] += 0.5 * tmp3[i - j - 1] * InitWeaponDensity[j - 1];
                }
            }
            for (int i = 0; i < dis.Length; i++)
            {
                res[i] += dis[i];
            }
            return res;
        }
    }
}
