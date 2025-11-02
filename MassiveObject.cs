using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema_EGC
{
    /// <summary>
    /// Clasa asta se ocupă de încărcarea și afișarea unui obiect 3D
    /// dintr-un fișier .obj. E mai complex decât un cubuleț simplu.
    /// </summary>
    public class MassiveObject
    {
        
        private const String FILENAME = "assets/slime.obj";
        //private const String FILENAME = "assets/volleyball.obj";

        // Majoritatea modelelor .obj sunt făcute la scară mică (ex: de la -1 la 1).
        // Ca să le vedem în scena noastră, trebuie să le mărim.
        private const int FACTOR_SCALARE_IMPORT = 100;

        // Lista în care o să băgăm toate vârfurile (vertex-urile) citite din fișier.
        private List<Vector3> coordsList;
        // Un flag simplu ca să știm dacă afișăm obiectul sau nu.
        private bool visibility;
        // Culoarea cu care o să desenăm obiectul (îl facem "solid color").
        private Color meshColor;
        
        private bool hasError;

        /// <summary>
        /// Constructorul clasei. Primește o culoare și încearcă să încarce fișierul .obj.
        /// </summary>
        public MassiveObject(Color col)
        {
            
            // și ar opri tot programul. Așa, doar prindem eroarea.
            try
            {
                // Apelăm funcția care face "munca grea": citește fișierul și ne dă lista de coordonate.
                coordsList = LoadFromObjFile(FILENAME);

                // Verificăm dacă funcția chiar a citit ceva.
                if (coordsList.Count == 0)
                {
                    // Dacă lista e goală, ceva nu e bine (poate fișierul .obj e gol sau are alt format).
                    Console.WriteLine("Crearea obiectului a esuat: obiect negasit/coordonate lipsa!");
                    return; // Ieșim din constructor.
                }

                // Dacă totul a mers bine, setăm starea inițială a obiectului.
                visibility = false; // Îl facem invizibil la început. Îl pornim noi dintr-o tastă (ex: 'O').
                meshColor = col; // Setăm culoarea primită.
                hasError = false; // Totul e ok, nu avem erori.
                Console.WriteLine("Obiect 3D încarcat - " + coordsList.Count.ToString() + " vertexuri disponibile!");
            }
            catch (Exception ex)
            {
                // Aici ajungem dacă 'try'-ul a eșuat (ex: 'FileNotFoundException').
                Console.WriteLine("ERROR: assets file <" + FILENAME + "> is missing!!!");
                hasError = true; // Setăm flag-ul de eroare.
            }
        }

       
        public void ToggleVisibility()
        {
            // Verificăm întâi dacă obiectul s-a încărcat corect.
            if (hasError == false)
            {
                // Inversăm valoarea booleană (true -> false, false -> true).
                visibility = !visibility;
            }
        }

        /// <summary>
        /// Metoda de desenare, chemată în 'OnRenderFrame' din 'Window3D'.
        /// </summary>
        public void Draw()
        {
            // Desenăm obiectul DOAR dacă nu are erori de încărcare ȘI dacă e setat ca vizibil.
            if (hasError == false && visibility == true)
            {
                GL.Color3(meshColor); // Setăm culoarea.
                // Majoritatea fișierelor .obj simple sunt făcute din triunghiuri.
                GL.Begin(PrimitiveType.Triangles);

                // Parcurgem toată lista de vârfuri pe care am citit-o...
                foreach (var vert in coordsList)
                {
                    // ...și trimitem fiecare vârf la OpenGL pentru desenare.
                    GL.Vertex3(vert);
                }
                GL.End(); // Am terminat de desenat triunghiurile.
            }
        }

        
        private List<Vector3> LoadFromObjFile(string fname)
        {
            // Creăm o listă nouă, temporară, unde să adăugăm ce citim.
            List<Vector3> vlc3 = new List<Vector3>();

            // Citim toate liniile din fișierul .obj.
            var lines = File.ReadLines(fname);

            // Trecem prin fiecare linie pe rând.
            foreach (var line in lines)
            {
                // Ignorăm liniile goale.
                if (line.Trim().Length > 2)
                {
                    // Luăm primele două caractere de pe linie (după ce am șters spațiile de la început/sfârșit).
                    string ch1 = line.Trim().Substring(0, 1);
                    string ch2 = line.Trim().Substring(1, 1);

                    // În formatul .obj, liniile cu vârfuri (coordonate)
                    // încep cu "v " (v-spațiu).
                    if (ch1 == "v" && ch2 == " ")
                    {
                        // Am găsit un vârf!
                        // Spargem linia în bucăți, folosind spațiul ca separator.
                        // Ex: "v 0.1 0.5 0.2" -> ["v", "0.1", "0.5", "0.2"]
                        string[] block = line.Trim().Split(' ');

                        // Ne asigurăm că avem exact 4 bucăți (v, x, y, z).
                        if (block.Length == 4)
                        {
                            
                            // 'float.Parse' de obicei se descurcă, dar e bine de știut.

                            // Extragem valorile și le înmulțim direct cu factorul de scalare.
                            float xval = float.Parse(block[1].Trim()) * FACTOR_SCALARE_IMPORT;
                            float yval = float.Parse(block[2].Trim()) * FACTOR_SCALARE_IMPORT;
                            float zval = float.Parse(block[3].Trim()) * FACTOR_SCALARE_IMPORT;

                           
                            // (se trunchiază zecimalele), dar poate e ok pentru ce avem nevoie.
                            vlc3.Add(new Vector3((int)xval, (int)yval, (int)zval));
                        }
                    }
                }
            }

            // Returnăm lista completă de vârfuri.
            return vlc3;
        }

    }
}