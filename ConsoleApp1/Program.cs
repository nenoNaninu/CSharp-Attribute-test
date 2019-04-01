using System;
using System.Reflection;

namespace ConsoleApp1
{
    //属性作る時はSystem.Attributeを継承する。
    //こんな感じで属性をなににつけるか決める。
    //[AttributeUsage(AttributeTargets.Property|AttributeTargets.Class)]//こんな感じで適用ヶ所を弄れる。
    [AttributeUsage(AttributeTargets.Property)]

    public abstract class RestrictionAttribute : System.Attribute
    {
        public virtual int MaxLen { set; get; }
        public virtual bool NumberOnly { set; get; }
        public abstract bool IsValid(object value);//ValidationAttributeというやつがSystem.ComponentModel.DataAnnotations空間にいるけどとりあえず練習なので
    }

    public class FieldRestrictionAttribute : RestrictionAttribute
    {
        public override int MaxLen { set; get; } = 20;
        public override bool NumberOnly { set; get; } = false;

        public override bool IsValid(object value)
        {
            if (!(value is string s))
            {
                return false;
            }

            if (NumberOnly)
            {
                if (!int.TryParse(s, out _))
                {
                    Console.WriteLine($"NumberOnly={NumberOnly}にも拘わらず数値以外を持ってます");

                    return false;
                }
            }

            if (MaxLen < s.Length)
            {
                Console.WriteLine($"MaxLen={MaxLen} を超えています");

                return false;
            }
            return true;
        }
    }

    public class User
    {
        private const int constValue = 20;

        [FieldRestriction(MaxLen = 10, NumberOnly = true)]
        public string NationalNumber { get; set; }

        /// <summary>
        /// 属性の値はコンパイル時定数じゃないとダメ。
        /// </summary>
        [FieldRestriction(MaxLen = constValue, NumberOnly = false)]
        public string Name { set; get; }
    }

    public static class RestrictionChecker
    {
        /// <summary>
        /// 設定された属性に違反していないか確認する
        /// </summary>
        public static bool Check<T, TAttribute>(T obj) where TAttribute : RestrictionAttribute
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                //属性が定義されたプロパティだけを参照するため、fixedAttrがnullなら処理の対象外
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(TAttribute)) is TAttribute attribute)
                {
                    //こういうstringべちゃっとした感じが嫌なので、
                    //IsVaildとか使うしたの関数みたいな実装をする。
                    if (!(propertyInfo.GetValue(obj) is string vaule))
                    {
                        continue;
                    }

                    if (attribute.NumberOnly)
                    {
                        if (!int.TryParse(vaule, out _))
                        {
                            Console.WriteLine($"{propertyInfo.Name}プロパティの属性値：NumberOnly={attribute.NumberOnly}にも拘わらず数値以外を持ってます");

                            return false;
                        }
                    }

                    if (attribute.MaxLen < vaule.Length)
                    {
                        Console.WriteLine($"{propertyInfo.Name}プロパティの属性値： MaxLen={attribute.MaxLen} を超えています");

                        return false;
                    }

                    Console.WriteLine($"{propertyInfo.Name}プロパティの属性値： MaxLen={attribute.MaxLen} NumberOnly={attribute.NumberOnly}");

                }
            }

            return true;
        }

        /// <summary>
        /// 設定された属性に違反していないか確認する
        /// </summary>
        public static bool CheckIsVaild<T, TAttribute>(T obj) where TAttribute : RestrictionAttribute
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                //属性が定義されたプロパティだけを参照するため、fixedAttrがnullなら処理の対象外
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(TAttribute)) is TAttribute attribute)
                {
                    if (!attribute.IsValid(propertyInfo.GetValue(obj)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var usr1 = new User()
            {
                Name = "HogeHuga",
                NationalNumber = "3333333"
            };

            var usr2 = new User()
            {
                Name = "HogeHuga",
                NationalNumber = "あ"
            };

            var usr3 = new User()
            {
                Name = "HogeHugaHogeHugaHogeHugaHogeHugaHogeHugaHogeHugaHogeHugaHogeHugaHogeHugaHogeHuga",
                NationalNumber = "33"
            };

            Console.WriteLine(RestrictionChecker.Check<User, FieldRestrictionAttribute>(usr1));
            Console.WriteLine(RestrictionChecker.Check<User, FieldRestrictionAttribute>(usr2));
            Console.WriteLine(RestrictionChecker.Check<User, FieldRestrictionAttribute>(usr3));
            Console.WriteLine("=========================");
            Console.WriteLine(RestrictionChecker.CheckIsVaild<User, FieldRestrictionAttribute>(usr1));
            Console.WriteLine(RestrictionChecker.CheckIsVaild<User, FieldRestrictionAttribute>(usr2));
            Console.WriteLine(RestrictionChecker.CheckIsVaild<User, FieldRestrictionAttribute>(usr3));

            Console.ReadLine();

        }
    }
}

