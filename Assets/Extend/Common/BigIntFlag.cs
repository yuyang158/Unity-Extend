using System.Linq;

namespace Extend.Common {
	public class BigIntFlag {
		private readonly byte[] m_flagBuffer;

		public BigIntFlag(int sizeInByte) {
			m_flagBuffer = new byte[sizeInByte];
		}

		public void Mark(int mask) {
			var bufferOffset = mask / 8;
			var byteOffset = mask % 8;

			m_flagBuffer[bufferOffset] |= (byte)(1 << byteOffset);
		}

		public void Unmark(int mask) {
			var bufferOffset = mask / 8;
			var byteOffset = mask % 8;
			m_flagBuffer[bufferOffset] &= (byte)~(1 << byteOffset);
		}

		public bool ZeroMark() {
			return m_flagBuffer.All(flag => flag == 0);
		}
	}
}