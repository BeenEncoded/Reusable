using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization;
using System.Linq;

using reusable.data;

namespace reusable.utility
{
    public static class constant
    {
        public static readonly Encoding defaultEncoding = Encoding.UTF8;

    }

    /// <summary>
    /// This is a countdown timer.  It counts down to zero, and allows you to check if it's 
    /// completed.  This is useful for waiting for things for a certain amount of time and
    /// things like that.
    /// </summary>
    public class BasicTimer
    {
        private DateTime start;
        private TimeSpan length;

        public BasicTimer(in long milliseconds)
        {
            /* Tick = 100 nanoseconds (T)
             * for 1 millisecond, M = T * 10000
             * for ticks, T = M / 10000
             */
            length = new TimeSpan(milliseconds * 10000);
        }

        /// <summary>
        /// Starts the timer.To reset the timer, simply start it again!
        /// </summary>
        public void StartTimer()
        {
            start = DateTime.Now;
        }

        /// <summary>
        /// Gets if the timer is finished or not.
        /// </summary>
        /// <returns>True if the time is elapsed.</returns>
        public bool AtEnd()
        {
            return (DateTime.Now - start) >= length;
        }

        /// <summary>
        /// Gets the time left on the timer.
        /// </summary>
        /// <returns>A timespan representing the time left on the timer.</returns>
        public TimeSpan TimeLeft()
        {
            return length - (DateTime.Now - start);
        }


    }

    public static class common
    {
        public delegate bool predicate_t();

        /// <summary>
        /// Assesses if the type is fundamental.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <returns>True if T is one of the fundamental types.</returns>
        public static bool is_fundamental_type<T>(in T value)
        {
            // Did I miss one?
            if(value == null) return false;
            return ((value.GetType() == typeof(int)) || (value.GetType() == typeof(char)) ||
                    (value.GetType() == typeof(long)) || (value.GetType() == typeof(ulong)) ||
                    (value.GetType() == typeof(uint)) || (value.GetType() == typeof(bool)) || 
                    (value.GetType() == typeof(byte)) || (value.GetType() == typeof(float)) || 
                    (value.GetType() == typeof(sbyte)) || (value.GetType() == typeof(double)) || 
                    (value.GetType() == typeof(short)) || (value.GetType() == typeof(ushort)) || 
                    (value.GetType() == typeof(decimal)));
        }

        public static byte[] strtobyte(in string s)
        {
            byte[] b = new byte[s.Length * sizeof(char)];
            Buffer.BlockCopy(s.ToCharArray(), 0, b, 0, b.Length);
            return b;
        }

        public static string bytetostr(in byte[] b)
        {
            char[] ch = new char[b.Length / sizeof(char)];
            Buffer.BlockCopy(b, 0, ch, 0, b.Length);
            return new string(ch);
        }

        /// <summary>
        /// Waits until the passed predicate returns true, for a finite length of time.
        /// </summary>
        /// <param name="p">The predicate to test.</param>
        /// <param name="t">The amount of time until the wait times out in milliseconds.</param>
        /// <param name="cyclespeed">How many times-per-second the predicate will be checked.</param>
        /// <returns>True if p() is true, otherwise false. </returns>
        public static bool wait_until(in predicate_t p, in uint t, ushort cyclespeed = 30)
        {
            BasicTimer timer = new BasicTimer(t);
            timer.StartTimer();
            do{
                Thread.Sleep(1000 / cyclespeed);
            }while(!timer.AtEnd() && !p());
            return p();
        }

        public static string exception_display(in Exception e)
        {
            return ("Message: " + e.Message + "\r\n" +
                    "Source: " + e.Source + "\r\n" +
                    "HRESULT: " + e.HResult + "\r\n" +
                    "Help: " + e.HelpLink + "\r\n\r\n" +
                    "Stack Trace: " + e.StackTrace);
        }


    }

    public struct pair<type1, type2>
    {
        private type1 f;
        private type2 s;
        
        public pair(type1 t1, type2 t2)
        {
            f = t1;
            s = t2;
        }
        
        public type1 first
        {
            get{return f;}
            set{f = value;}
        }

        public type2 second
        {
            get{return s;}
            set{s = value;}
        }


    }

    public static class io
    {
        public class TypeUnsupportedException : Exception
        {
            public TypeUnsupportedException()
            {
            }

            public TypeUnsupportedException(string message) : base(message)
            {
            }

            public TypeUnsupportedException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected TypeUnsupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        /// <summary>
        /// Adds a much-neeeded type-agnostic read function for fundamental
        /// types to the BinaryReader object.
        /// </summary>
        /// <typeparam name="T">The type to read.</typeparam>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="value">The value to read into.</param>
        /// <returns>The type.</returns>
        public static bool read<T>(this BinaryReader reader, out T value)
        {
            if(!common.is_fundamental_type<T>(default))
            {
                throw new TypeUnsupportedException($"{typeof(T).FullName} is not supported by the BinaryReader.read!");
            }
            //Until I find a type-agnostic way of making these calls, This
            // will have to do...  If the compiler does anything like clang or
            //gcc this should at least translate to a jump-table...
            //ok here we go:
            value = default(T);
            try
            {
                //shield your eyes!
                if (value.GetType() == typeof(int))
                {
                    value = (dynamic)reader.ReadInt32();
                }
                else if (value.GetType() == typeof(char))
                {
                    value = (dynamic)reader.ReadChar();
                }
                else if (value.GetType() == typeof(long))
                {
                    value = (dynamic)reader.ReadInt64();
                }
                else if (value.GetType() == typeof(ulong))
                {
                    value = (dynamic)reader.ReadUInt64();
                }
                else if (value.GetType() == typeof(uint))
                {
                    value = (dynamic)reader.ReadUInt32();
                }
                else if (value.GetType() == typeof(bool))
                {
                    value = (dynamic)reader.ReadBoolean();
                }
                else if (value.GetType() == typeof(byte))
                {
                    value = (dynamic)reader.ReadByte();
                }
                else if(value.GetType() == typeof(float))
                {
                    value = (dynamic)reader.ReadSingle();
                }
                else if(value.GetType() == typeof(sbyte))
                {
                    value = (dynamic)reader.ReadSByte();
                }
                else if(value.GetType() == typeof(double))
                {
                    value = (dynamic)reader.ReadDouble();
                }
                else if(value.GetType() == typeof(short))
                {
                    value = (dynamic)reader.ReadInt16();
                }
                else if(value.GetType() == typeof(ushort))
                {
                    value = (dynamic)reader.ReadUInt16();
                }
                else if(value.GetType() == typeof(decimal))
                {
                    value = (dynamic)reader.ReadDecimal();
                }
                return true;
            }
            catch(EndOfStreamException)
            {
            }
            return false;
        }

        /// <summary>
        /// A generic write function that can replace the overloads.  This 
        /// was added becauce type assessment failed when using dynamic.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="writer">the writer to write to</param>
        /// <param name="value">the value to write.</param>
        /// <returns></returns>
        public static void write<T>(this BinaryWriter writer, in T value)
        {
            if(!common.is_fundamental_type(value))
            {
                throw new TypeUnsupportedException($"{typeof(T).FullName} is not supported by this write function.");
            }

            if (value.GetType() == typeof(int))
            {
                int t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(char))
            {
                char t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(long))
            {
                long t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(ulong))
            {
                ulong t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(uint))
            {
                uint t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(bool))
            {
                bool t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(byte))
            {
                byte t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(float))
            {
                float t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(sbyte))
            {
                sbyte t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(double))
            {
                double t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(short))
            {
                short t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(ushort))
            {
                ushort t = (dynamic)value;
                writer.Write(t);
            }
            else if (value.GetType() == typeof(decimal))
            {
                decimal t = (dynamic)value;
                writer.Write(t);
            }
            else
            {
                throw new TypeUnsupportedException($"{typeof(T).FullName} is not supported by this write function.");
            }
        }

        /// <summary>
        /// Writes the value to a binary writer.  This function essentially serializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of value to write.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">The data to serialize.</param>
        /// <param name="writer">The writer to use.</param>
        /// <param name="encoding">The encoding to use.  UTF8 recommended for the broadest support.</param>
        public static void write<T>(in T value, in BinaryWriter writer, Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = constant.defaultEncoding;
            }
            if(common.is_fundamental_type(value))
            {
                // this should be fine.
                // carry on.
                writer.write<T>(value);
            }
            else if(typeof(string) == value.GetType())
            {
                byte[] data = encoding.GetBytes(value as string);
                writer.write(data.Length);
                if(data.Length > 0) writer.Write(data);
            }
            else if(value is RWObject)
            {
                RWObject s = (dynamic)value;
                s.write(writer, encoding);
            }
            else
            {
                throw new TypeUnsupportedException($"{typeof(T).FullName} not supported by write (non-list overload)!");
            }
        }

        /// <summary>
        /// Writes the value to a binary writer.  This function essentially serializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of value to write.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">The data to serialize.</param>
        /// <param name="writer">The writer to use.</param>
        /// <param name="encoding">The encoding to use.  UTF8 recommended for the broadest support.
        public static void write<T>(in T value, in Stream s, Encoding encoding = null)
        {
            using(BinaryWriter writer = new BinaryWriter(s, encoding, true))
            {
                write<T>(value, writer, encoding);
            }
        }

        /// <summary>
        /// Writes a list to a binary stream.  This function essentially serializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of the list to write.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">A List of type T to write to the stream.</param>
        /// <param name="writer">The writer to use.</param>
        /// <param name="encoding">The encoding to use.</param>
        public static void writelist<T>(in List<T> value, in BinaryWriter writer, Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = constant.defaultEncoding;
            }

            write<int>(value.Count, writer, encoding);
            if(value.Count > 0)
            {
                foreach(T element in value)
                {
                    write(in element, writer, encoding);
                }
            }
        }

        /// <summary>
        /// Write a list to a stream using a BinaryWriter.  This function essentially serializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of the list to write.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">A List of type T to write to the stream.</param>
        /// <param name="s">The stream to write to.</param>
        /// <param name="encoding"></param>
        public static void writelist<T>(in List<T> value, in Stream s, Encoding encoding = null)
        {
            using(BinaryWriter writer = new BinaryWriter(s, encoding, true))
            {
                writelist<T>(in value, writer, encoding);
            }
        }

        /// <summary>
        /// Reads a value from a binary reader.  This function essentially deserializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">The value to read -- of type T.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="encoding">the encoding to use.  UTF8 recommended.</param>
        /// <returns>True if -- and only if -- the value was read.  False if the end of the stream was
        /// reached or any other error occured.</returns>
        public static bool read<T>(out T value, in BinaryReader reader, Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = constant.defaultEncoding;
            }

            value = default(T);
            if(typeof(T).FullName == "System.String") // default(T) is null, so we need to hack it a bit
            {
                if(reader.read(out int count))
                {
                    value = (dynamic)encoding.GetString(reader.ReadBytes(count));
                    return true;
                }
            }
            else if(typeof(T).GetInterfaces().Any(x => x.FullName == typeof(RWObject).FullName))
            {
                if(value == null)
                {
                    value = (T)Activator.CreateInstance(typeof(T));
                }

                /* We have to do this stupid object assignment or value 
                 * is never modified. */
                RWObject ob = (dynamic)value;
                bool result = ob.read(reader, encoding);
                value = (dynamic)ob;
                return result;
            }
            else if (common.is_fundamental_type(value))
            {
                return reader.read(out value);
            }
            else
            {
                throw new TypeUnsupportedException($"{typeof(T).FullName} is not supported by write!");
            }
            return false;
        }

        /// <summary>
        /// Reads a value from a stream using a binary reader.  This function essentially deserializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">The value to read -- of type T.</param>
        /// <param name="s">The stream to read from.</param>
        /// <param name="encoding">the Encoding to use -- UTF8 recommended.</param>
        /// <returns>True if -- and only if -- the value was read.  False if the end of the stream was
        /// reached or any other error occured.</returns>
        public static bool read<T>(out T value, in Stream s, Encoding encoding = null)
        {
            using(BinaryReader reader = new BinaryReader(s, encoding, true))
            {
                return read(out value, reader, encoding);
            }
        }

        /// <summary>
        /// Reads a list of type T from a BinaryReader using the specified encoding.
        /// This function essentially deserializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of the List to read.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">The List[T] to read.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="encoding">The encoding to use -- UTF8 recommended.</param>
        /// <returns>True if -- and only if -- the list was read.  False if the end of the stream was
        /// reached or any other error occured.</returns>
        public static bool readlist<T>(out List<T> value, in BinaryReader reader, Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = constant.defaultEncoding;
            }

            value = new List<T>();
            if(read(out int len, reader))
            {
                if(len > 0)
                {
                    T tempval;
                    for(int x = 0; (x < len); ++x)
                    {
                        if(read<T>(out tempval, reader, encoding))
                            value.Add(tempval);
                        else
                            break;
                    }
                    return len == value.Count;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads a list of type T from a stream using a BinaryReader.
        /// This function essentially deserializes the object in a way
        /// that is both length and type agnostic.
        /// </summary>
        /// <typeparam name="T">The type of the List to read.  Must only be an object that inherits from RWObject,
        /// a fundamental type, or a string.</typeparam>
        /// <param name="value">The List[T] to read.</param>
        /// <param name="s">The stream from which to read.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>True if -- and only if -- the list was read.  False if the end of the stream was
        /// reached or any other error occured.</returns>
        public static bool readlist<T>(out List<T> value, in Stream s, Encoding encoding = null)
        {
            using(BinaryReader reader = new BinaryReader(s, encoding, true))
            {
                return readlist(out value, reader, encoding);
            }
        }



    }
}