using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Utils;

namespace Bilibili.AspNetCore.Apis.Services.Cookie
{
    class Buvid3_4Calculator
    {
        private BigInteger MOD = ((BigInteger)1) << 64;
        private BigInteger C1 = 0x87C37B91114253D5;
        private BigInteger C2 = 0x4CF5AD432745937F;
        private BigInteger C3 = 0x52DCE729;
        private BigInteger C4 = 0x38495AB5;
        const int R1 = 27, R2 = 31, R3 = 33, M = 5;

        private string uuid;
        private ulong ts;
        private int seed;

        public Buvid3_4Calculator(string uuid, ulong ts, int seed = 31)
        {
            this.uuid = uuid;
            this.ts = ts * 1000;
            this.seed = seed;
        }

        public string Generate()
        {
            string payload = GetPayload();
            using (MemoryStream source = new MemoryStream(Encoding.ASCII.GetBytes(payload)))
            {
                BigInteger m = Murmur3_x64_128(source, seed);
                return $"{m & (MOD - 1):x}{m >> 64:x}";
            }
        }

        private BigInteger RotateLeft(BigInteger x, int k)
        {
            string binaryString = string.Join(string.Empty, x.ToString("X").Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
            binaryString = binaryString.Length > 64 ? binaryString.Substring(binaryString.Length - 64, 64) : binaryString.PadLeft(64, '0');
            string rotatedStr = binaryString.Substring(k) + binaryString.Substring(0, k);
            BigInteger value = 0;
            foreach (char c in rotatedStr)
            {
                value <<= 1;
                value += c == '1' ? 1 : 0;
            }
            return value;
        }

        private BigInteger Murmur3_x64_128(MemoryStream source, int seed)
        {
            BigInteger h1 = seed, h2 = seed;
            int processed = 0;
            while (true)
            {
                byte[] read = new byte[16];
                int bytesRead = source.Read(read, 0, 16);
                processed += bytesRead;
                if (bytesRead == 16)
                {
                    BigInteger k1 = BitConverter.ToUInt64(read, 0);
                    BigInteger k2 = BitConverter.ToUInt64(read, 8);
                    h1 ^= RotateLeft(k1 * C1 % MOD, R2) * C2 % MOD;
                    h1 = ((RotateLeft(h1, R1) + h2) * M + C3) % MOD;
                    h2 ^= RotateLeft(k2 * C2 % MOD, R3) * C1 % MOD;
                    h2 = ((RotateLeft(h2, R2) + h1) * M + C4) % MOD;
                }
                else if (bytesRead == 0)
                {
                    h1 ^= processed;
                    h2 ^= processed;
                    h1 = (h1 + h2) % MOD;
                    h2 = (h2 + h1) % MOD;
                    h1 = Fmix64(h1);
                    h2 = Fmix64(h2);
                    h1 = (h1 + h2) % MOD;
                    h2 = (h2 + h1) % MOD;
                    return (h2 << 64) | h1;
                }
                else
                {
                    BigInteger k1 = 0, k2 = 0;
                    if (bytesRead >= 15)
                        k2 ^= read[14] << 48;
                    if (bytesRead >= 14)
                        k2 ^= read[13] << 40;
                    if (bytesRead >= 13)
                        k2 ^= read[12] << 32;
                    if (bytesRead >= 12)
                        k2 ^= read[11] << 24;
                    if (bytesRead >= 11)
                        k2 ^= read[10] << 16;
                    if (bytesRead >= 10)
                        k2 ^= read[9] << 8;
                    if (bytesRead >= 9)
                    {
                        k2 ^= read[8];
                        k2 = RotateLeft(k2 * C2 % MOD, R3) * C1 % MOD;
                        h2 ^= k2;
                    }
                    if (bytesRead >= 8)
                        k1 ^= read[7] << 56;
                    if (bytesRead >= 7)
                        k1 ^= read[6] << 48;
                    if (bytesRead >= 6)
                        k1 ^= read[5] << 40;
                    if (bytesRead >= 5)
                        k1 ^= read[4] << 32;
                    if (bytesRead >= 4)
                        k1 ^= read[3] << 24;
                    if (bytesRead >= 3)
                        k1 ^= read[2] << 16;
                    if (bytesRead >= 2)
                        k1 ^= read[1] << 8;
                    if (bytesRead >= 1)
                        k1 ^= read[0];
                    k1 = RotateLeft(k1 * C1 % MOD, R2) * C2 % MOD;
                    h1 ^= k1;
                }
            }
        }

        private BigInteger Fmix64(BigInteger k)
        {
            const ulong C1 = 0xFF51AFD7ED558CCD;
            const ulong C2 = 0xC4CEB9FE1A85EC53;
            const int R = 33;
            k ^= k >> R;
            k = k * C1 % MOD;
            k ^= k >> R;
            k = k * C2 % MOD;
            k ^= k >> R;
            return k;
        }

        public string GetPayload()
        {
            string payloadJson = "{\"3064\":1,\"5062\":\"" + ts.ToString()
                + "\",\"03bf\":\"https%3A%2F%2Fwww.bilibili.com%2F\",\"39c8\":\"333.1007.fp.risk\",\"34f1\":\"\",\"d402\":\"\",\"654a\":\"\",\"6e7c\":\"839x959\",\"3c43\":{\"2673\":1,\"5766\":24,\"6527\":0,\"7003\":1,\"807e\":1,\"b8ce\":\"" + GlobalConfigConstant.USER_AGENT
                + "\",\"641c\":0,\"07a4\":\"zh-CN\",\"1c57\":8,\"0bd0\":16,\"748e\":[2560,1080],\"d61f\":[2467,1091],\"fc9d\":480,\"6aa9\":\"Asia/Shanghai\",\"75b8\":1,\"3b21\":1,\"8a1c\":0,\"d52f\":\"not available\",\"adca\":\"Win32\",\"80c9\":[[\"PDF Viewer\",\"Portable Document Format\",[[\"application/pdf\",\"pdf\"],[\"text/pdf\",\"pdf\"]]],[\"Chrome PDF Viewer\",\"Portable Document Format\",[[\"application/pdf\",\"pdf\"],[\"text/pdf\",\"pdf\"]]],[\"Chromium PDF Viewer\",\"Portable Document Format\",[[\"application/pdf\",\"pdf\"],[\"text/pdf\",\"pdf\"]]],[\"Microsoft Edge PDF Viewer\",\"Portable Document Format\",[[\"application/pdf\",\"pdf\"],[\"text/pdf\",\"pdf\"]]],[\"WebKit built-in PDF\",\"Portable Document Format\",[[\"application/pdf\",\"pdf\"],[\"text/pdf\",\"pdf\"]]]],\"13ab\":\"mTUAAAAASUVORK5CYII=\",\"bfe9\":\"aTot0S1jJ7Ws0JC6QkvAL/A4H1PbV+/QA3AAAAAElFTkSuQmCC\",\"a3c1\":[\"extensions:ANGLE_instanced_arrays;EXT_blend_minmax;EXT_color_buffer_half_float;EXT_disjoint_timer_query;EXT_float_blend;EXT_frag_depth;EXT_shader_texture_lod;EXT_texture_compression_bptc;EXT_texture_compression_rgtc;EXT_texture_filter_anisotropic;EXT_sRGB;KHR_parallel_shader_compile;OES_element_index_uint;OES_fbo_render_mipmap;OES_standard_derivatives;OES_texture_float;OES_texture_float_linear;OES_texture_half_float;OES_texture_half_float_linear;OES_vertex_array_object;WEBGL_color_buffer_float;WEBGL_compressed_texture_s3tc;WEBGL_compressed_texture_s3tc_srgb;WEBGL_debug_renderer_info;WEBGL_debug_shaders;WEBGL_depth_texture;WEBGL_draw_buffers;WEBGL_lose_context;WEBGL_multi_draw\",\"webgl aliased line width range:[1, 1]\",\"webgl aliased point size range:[1, 1024]\",\"webgl alpha bits:8\",\"webgl antialiasing:yes\",\"webgl blue bits:8\",\"webgl depth bits:24\",\"webgl green bits:8\",\"webgl max anisotropy:16\",\"webgl max combined texture image units:32\",\"webgl max cube map texture size:16384\",\"webgl max fragment uniform vectors:1024\",\"webgl max render buffer size:16384\",\"webgl max texture image units:16\",\"webgl max texture size:16384\",\"webgl max varying vectors:30\",\"webgl max vertex attribs:16\",\"webgl max vertex texture image units:16\",\"webgl max vertex uniform vectors:4095\",\"webgl max viewport dims:[32767, 32767]\",\"webgl red bits:8\",\"webgl renderer:WebKit WebGL\",\"webgl shading language version:WebGL GLSL ES 1.0 (OpenGL ES GLSL ES 1.0 Chromium)\",\"webgl stencil bits:0\",\"webgl vendor:WebKit\",\"webgl version:WebGL 1.0 (OpenGL ES 2.0 Chromium)\",\"webgl unmasked vendor:Google Inc. (NVIDIA) #X3fQVPgERx\",\"webgl unmasked renderer:ANGLE (NVIDIA, NVIDIA GeForce RTX 3060 Laptop GPU (0x00002560) Direct3D11 vs_5_0 ps_5_0, D3D11) #X3fQVPgERx\",\"webgl vertex shader high float precision:23\",\"webgl vertex shader high float precision rangeMin:127\",\"webgl vertex shader high float precision rangeMax:127\",\"webgl vertex shader medium float precision:23\",\"webgl vertex shader medium float precision rangeMin:127\",\"webgl vertex shader medium float precision rangeMax:127\",\"webgl vertex shader low float precision:23\",\"webgl vertex shader low float precision rangeMin:127\",\"webgl vertex shader low float precision rangeMax:127\",\"webgl fragment shader high float precision:23\",\"webgl fragment shader high float precision rangeMin:127\",\"webgl fragment shader high float precision rangeMax:127\",\"webgl fragment shader medium float precision:23\",\"webgl fragment shader medium float precision rangeMin:127\",\"webgl fragment shader medium float precision rangeMax:127\",\"webgl fragment shader low float precision:23\",\"webgl fragment shader low float precision rangeMin:127\",\"webgl fragment shader low float precision rangeMax:127\",\"webgl vertex shader high int precision:0\",\"webgl vertex shader high int precision rangeMin:31\",\"webgl vertex shader high int precision rangeMax:30\",\"webgl vertex shader medium int precision:0\",\"webgl vertex shader medium int precision rangeMin:31\",\"webgl vertex shader medium int precision rangeMax:30\",\"webgl vertex shader low int precision:0\",\"webgl vertex shader low int precision rangeMin:31\",\"webgl vertex shader low int precision rangeMax:30\",\"webgl fragment shader high int precision:0\",\"webgl fragment shader high int precision rangeMin:31\",\"webgl fragment shader high int precision rangeMax:30\",\"webgl fragment shader medium int precision:0\",\"webgl fragment shader medium int precision rangeMin:31\",\"webgl fragment shader medium int precision rangeMax:30\",\"webgl fragment shader low int precision:0\",\"webgl fragment shader low int precision rangeMin:31\",\"webgl fragment shader low int precision rangeMax:30\"],\"6bc5\":\"Google Inc. (NVIDIA) #X3fQVPgERx~ANGLE (NVIDIA, NVIDIA GeForce RTX 3060 Laptop GPU (0x00002560) Direct3D11 vs_5_0 ps_5_0, D3D11) #X3fQVPgERx\",\"ed31\":0,\"72bd\":0,\"097b\":0,\"52cd\":[0,0,0],\"a658\":[\"Arial\",\"Arial Black\",\"Arial Narrow\",\"Book Antiqua\",\"Bookman Old Style\",\"Calibri\",\"Cambria\",\"Cambria Math\",\"Century\",\"Century Gothic\",\"Century Schoolbook\",\"Comic Sans MS\",\"Consolas\",\"Courier\",\"Courier New\",\"Georgia\",\"Helvetica\",\"Helvetica Neue\",\"Impact\",\"Lucida Bright\",\"Lucida Calligraphy\",\"Lucida Console\",\"Lucida Fax\",\"Lucida Handwriting\",\"Lucida Sans\",\"Lucida Sans Typewriter\",\"Lucida Sans Unicode\",\"Microsoft Sans Serif\",\"Monotype Corsiva\",\"MS Gothic\",\"MS PGothic\",\"MS Reference Sans Serif\",\"MS Sans Serif\",\"MS Serif\",\"Palatino Linotype\",\"Segoe Print\",\"Segoe Script\",\"Segoe UI\",\"Segoe UI Light\",\"Segoe UI Semibold\",\"Segoe UI Symbol\",\"Tahoma\",\"Times\",\"Times New Roman\",\"Trebuchet MS\",\"Verdana\",\"Wingdings\",\"Wingdings 2\",\"Wingdings 3\"],\"d02f\":\"124.04347527516074\"},\"54ef\":\"{\\\"b_ut\\\":\\\"7\\\",\\\"home_version\\\":\\\"V8\\\",\\\"i-wanna-go-back\\\":\\\"-1\\\",\\\"in_new_ab\\\":true,\\\"ab_version\\\":{\\\"for_ai_home_version\\\":\\\"V8\\\",\\\"tianma_banner_inline\\\":\\\"CONTROL\\\",\\\"enable_web_push\\\":\\\"DISABLE\\\"},\\\"ab_split_num\\\":{\\\"for_ai_home_version\\\":54,\\\"tianma_banner_inline\\\":54,\\\"enable_web_push\\\":10}}\",\"8b94\":\"\",\"df35\":\"" + uuid
                + "\",\"07a4\":\"zh-CN\",\"5f45\":null,\"db46\":0}";

            var payloadObj = new
            {
                payload = payloadJson,
            };

            string payload = JsonUtil.SerializeObject(payloadObj);
            return payload;
        }
    }
}
