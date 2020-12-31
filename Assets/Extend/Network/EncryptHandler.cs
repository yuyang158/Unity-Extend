namespace Extend.Network
{
    public enum EncryptType
    {
        NONE, ARC4,     //
    }
    /// <summary>
    /// 加密处理基类，不同加密方式继承此类即可。需要注意，加密/解密会对原
    /// </summary>
    public abstract class EncryptHandler
    {
        public EncryptType m_type;
        public byte[] m_args;
        public EncryptHandler(EncryptType t, byte[] args)
        {
            m_type = t;
            m_args = args;
        }
        public abstract void Encrypt(ref byte[] plaintext);
        public abstract void Decrypt(ref byte[] ciphertext);
    }

    public class EncryptNoneHandler : EncryptHandler
    {
        public EncryptNoneHandler()
            :base(EncryptType.NONE, null)
        {
        }

        public override void Encrypt(ref byte[] plaintext)
        {
        }

        public override void Decrypt(ref byte[] ciphertext)
        {
        }
    }

    public class EncryptArc4Handler : EncryptHandler
    {
        private readonly byte[] _keyStream = new byte[256];
        private byte _index1;
        private byte _index2;
        public EncryptArc4Handler(byte[] args)
            :base(EncryptType.ARC4, args)
        {
            GenerateKeyStream();
        }
        
        public override void Encrypt(ref byte[] plaintext)
        {
            Calculate(plaintext);
        }

        public override void Decrypt(ref byte[] ciphertext)
        {
            Calculate(ciphertext);
        }

        public void UpdateKey(byte[] key)
        {
            m_args = key;
            GenerateKeyStream();
        }
        private void GenerateKeyStream()
        {
            int keylen = m_args.Length;
            for (int i = 0; i < 256; i++)
            {
                _keyStream[i] = (byte)i;
            }
            byte j = 0;
            for (int i = 0; i < 256; i++)
            {
                j += _keyStream[i];
                j += m_args[i % keylen];
                byte k = _keyStream[i]; _keyStream[i] = _keyStream[j]; _keyStream[j] = k;
            }
            _index1 = _index2 = 0;
        }

        private void Calculate(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                byte j1 = _keyStream[++_index1];
                byte j2 = _keyStream[_index2 += j1];
                _keyStream[_index2] = j1;
                _keyStream[_index1] = j2;
                data[i] ^= _keyStream[(byte) (j1 + j2)];
            }
        }
    }
}