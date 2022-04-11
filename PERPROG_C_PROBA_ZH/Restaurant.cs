using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PERPROG_C_PROBA_ZH
{
    class Utils
    {
        public static Random rnd = new Random();
    }
    class Restaurant
    {
        static List<Order> Orders = new List<Order>();
        static object OrdersLock = new object();
        private static bool Nyitva = true;
        static Courier[] Couriers = new Courier[8];
        public Restaurant(int n)
        {
            Nyitva = true;
            (new Task(()=>DoWork(),TaskCreationOptions.LongRunning)).Start();
            for (int i = 0; i < 8; i++)
            {
                if (i<4)
                {
                    Couriers[i] = new Courier(i + 1, Courier.Courier_Type.FürgeFutár);
                }
                else
                {
                    Couriers[i] = new Courier(i + 1, Courier.Courier_Type.TurbóTeknős);
                }
            }
            (new Task(()=>Kiiro(),TaskCreationOptions.LongRunning)).Start();
        }
        private void Kiiro()
        {
            while (Nyitva)
            {
                Console.Clear();
                for (int i = 0; i < Couriers.Length; i++)
                {
                    Console.WriteLine(Couriers[i].ToString());
                }
                Console.WriteLine("Várakozó Rendelések:");
                lock (OrdersLock)
                {
                    foreach (var item in Orders)
                    {
                        Console.WriteLine(item.ToString());
                    }  
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        private void DoWork()
        {
            for (int i = 0; i < 100; i++)//100db rendelés
            {
                System.Threading.Thread.Sleep(Utils.rnd.Next(1000, 5000));
                lock (OrdersLock)
                {
                    Orders.Add(new Order(i,Utils.rnd.Next(2000, 10001), Utils.rnd.Next(500, 10000)));
                }
            }
            while (Nyitva)
            {
                System.Threading.Thread.Sleep(1000);
                lock (OrdersLock)
                {
                    if (Orders.Count==0)
                    {
                        Nyitva = false;
                    }
                }
            }
       }
        class Order
        {
            int price;
            int distance;
            public int Distance { get; private set; }
            public int Price { get; private set; }
            public int Id { get; }

            public Order(int id,int price, int distance)
            {
                this.Id = id;
                Price = price;
                Distance = distance;
            }
            public override string ToString()
            {
                return "Id: " + Id + " " + Distance + "m " + Price + " Ft";
            }
        }
        class Courier
        {
            public int Id { get; }
            public int HatralevoTavolsag { get; private set; }
            public Order HisOrder { get; private set; }
            public Courier_Type Type { get; }
            public int Fizetés { get; private set; }
            public enum Courier_Type {FürgeFutár, TurbóTeknős}
            public Courier(int id,Courier_Type t)
            {
                Id = id;
                Fizetés = 0;
                HisOrder = null;
                Type = t;
                (new Task(() => DoWork(), TaskCreationOptions.LongRunning)).Start();
            }
            public override string ToString()
            {
                string kimenet=Type.ToString()+" "+Id+" Fizetés: "+Fizetés+"Ft "+ "HatralevoTavolsag " + HatralevoTavolsag + "m |";
                if (HisOrder!=null)
                {
                    kimenet += HisOrder.ToString();
                }
                return kimenet;
            }
            private void DoWork()
            {
                while (Nyitva)
                {
                    lock (OrdersLock) //A listát lockoljuk
                    {
                        if (Orders.Count>0)//Ha van benne akkor érdemes csinálni bármit
                        {
                            if (this.Type==Courier_Type.TurbóTeknős) //Ha TurbóTeknős
                            {
                                var v = Orders.OrderByDescending(t => t.Distance).First();
                                this.HisOrder = v;
                                Orders.Remove(v);
                                this.HatralevoTavolsag = 0;
                            }
                            else //Ha FürgeFutár
                            {
                                var v = Orders.OrderBy(t => t.Distance).First();
                                this.HisOrder = v;
                                Orders.Remove(v);
                                this.HatralevoTavolsag = 0;
                            }
                        }
                    }
                    if (this.HisOrder != null) //Ha van kész rendelés kiszállítja
                    {
                        System.Threading.Thread.Sleep(Utils.rnd.Next(2000, 5001));//Rendelés átadása
                        int distance = HisOrder.Distance;
                        HatralevoTavolsag = distance;
                        while (distance>0)
                        {
                            System.Threading.Thread.Sleep(Utils.rnd.Next(2000,3901));
                            distance -= 1000;
                            if (distance<0)
                            {
                                distance = 0;
                            }
                            HatralevoTavolsag = distance; 
                        }
                        if (this.Type == Courier_Type.FürgeFutár)
                        {
                            Fizetés += 600;
                            if (HisOrder.Distance>3000)
                            {
                                Fizetés += (int)(((HisOrder.Distance - 3000) / 1000)*200);
                            }
                        }
                        else
                        {
                            Fizetés += HisOrder.Price / 20;
                        }
                        this.HisOrder = null;
                    }
                    else//Ha nincs kész rendelés vár 1-2 percet
                    {
                        System.Threading.Thread.Sleep(Utils.rnd.Next(1000, 2001));
                    }
                }
            }
        }
    }
}
