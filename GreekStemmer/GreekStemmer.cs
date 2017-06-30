// Algorithm: Porter Word Stemmer by Martin Porter.
// Adapted for the greek language by George Ntais (original paper https://people.dsv.su.se/~hercules/papers/Ntais_greek_stemmer_thesis_final.pdf)
// C# port by Charalambos Theodorou

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Text;
public class GreekStemmer
{

    Assembly _assembly;
    StreamReader _configStreamReader;

    StemmerConfig config;
    Dictionary<string, string> exceptions;
    string[] protectedWords;
    Regex alphabet;

    public GreekStemmer()
    {
        _assembly = Assembly.GetExecutingAssembly();
        _configStreamReader = new StreamReader(_assembly.GetManifestResourceStream("GreekStemmer.StemmerConfig.json"));

        config = JsonConvert.DeserializeObject<StemmerConfig>(_configStreamReader.ReadToEnd());
        exceptions = config.exceptions;
        protectedWords = config.protectedwords;
        alphabet = new Regex("^[ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ]+$");
    }

    public string Stem(string word)
    {

        var stemmedWord = ConvertAndNormalize(word);

        if (stemmedWord.Length < 3)
        {
            return stemmedWord;
        }

        if (!isGreek(stemmedWord))
        {
            return stemmedWord;
        }

        if (Array.IndexOf(protectedWords, stemmedWord) >= 0)
        {
            return stemmedWord;
        }

        //step 1
        var stepOneRegExp = new Regex("(.*)(" + string.Join("|", exceptions.Keys) + ")$");
        var match = stepOneRegExp.Matches(stemmedWord);

        if (match != null && match.Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value + exceptions[match[0].Groups[2].Value];
        }
        //step 2
        //2a
        if ((match = new Regex("^(.+?)(ΑΔΕΣ|ΑΔΩΝ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (!Regex.IsMatch(match[0].Groups[1].Value, "(ΟΚ|ΜΑΜ|ΜΑΝ|ΜΠΑΜΠ|ΠΑΤΕΡ|ΓΙΑΓΙ|ΝΤΑΝΤ|ΚΥΡ|ΘΕΙ|ΠΕΘΕΡ|ΜΟΥΣΑΜ|ΚΑΠΛΑΜ|ΠΑΡ|ΨΑΡ|ΤΖΟΥΡ|ΤΑΜΠΟΥΡ|ΓΑΛΑΤ|ΦΑΦΛΑΤ)$"))
            {
                stemmedWord += "ΑΔ";
            }
        }

        //2b
        if ((match = new Regex("^(.+?)(ΕΔΕΣ|ΕΔΩΝ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "(ΟΠ|ΙΠ|ΕΜΠ|ΥΠ|ΓΗΠ|ΔΑΠ|ΚΡΑΣΠ|ΜΙΛ)$"))
            {
                stemmedWord += "ΕΔ";
            }
        }

        //2c
        if ((match = new Regex("^(.+?)(ΟΥΔΕΣ|ΟΥΔΩΝ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "(ΑΡΚ|ΚΑΛΙΑΚ|ΠΕΤΑΛ|ΛΙΧ|ΠΛΕΞ|ΣΚ|Σ|ΦΛ|ΦΡ|ΒΕΛ|ΛΟΥΛ|ΧΝ|ΣΠ|ΤΡΑΓ|ΦΕ)$"))
            {
                stemmedWord += "ΟΥΔ";
            }
        }

        //2d
        if ((match = new Regex("^(.+?)(ΕΩΣ|ΕΩΝ|ΕΑΣ|ΕΑ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(Θ|Δ|ΕΛ|ΓΑΛ|Ν|Π|ΙΔ|ΠΑΡ|ΣΤΕΡ|ΟΡΦ|ΑΝΔΡ|ΑΝΤΡ)$"))
            {
                stemmedWord += "Ε";
            }
        }

        //step 3
        //3a         
        if ((match = new Regex("^(.+?)(ΕΙΟ|ΕΙΟΣ|ΕΙΟΙ|ΕΙΑ|ΕΙΑΣ|ΕΙΕΣ|ΕΙΟΥ|ΕΙΟΥΣ|ΕΙΩΝ)$").Matches(stemmedWord)).Count > 0 && match[0].Groups[1].Value.Length > 4)
        {
            stemmedWord = match[0].Groups[1].Value;
        }

        //3b
        if ((match = new Regex("^(.+?)(ΙΟΥΣ|ΙΑΣ|ΙΕΣ|ΙΟΣ|ΙΟΥ|ΙΟΙ|ΙΩΝ|ΙΟΝ|ΙΑ|ΙΟ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (endsInVowel(stemmedWord) || stemmedWord.Length < 2 || new Regex("^(ΑΓ|ΑΓΓΕΛ|ΑΓΡ|ΑΕΡ|ΑΘΛ|ΑΚΟΥΣ|ΑΞ|ΑΣ|Β|ΒΙΒΛ|ΒΥΤ|Γ|ΓΙΑΓ|ΓΩΝ|Δ|ΔΑΝ|ΔΗΛ|ΔΗΜ|ΔΟΚΙΜ|ΕΛ|ΖΑΧΑΡ|ΗΛ|ΗΠ|ΙΔ|ΙΣΚ|ΙΣΤ|ΙΟΝ|ΙΩΝ|ΚΙΜΩΛ|ΚΟΛΟΝ|ΚΟΡ|ΚΤΗΡ|ΚΥΡ|ΛΑΓ|ΛΟΓ|ΜΑΓ|ΜΠΑΝ|ΜΠΡ|ΝΑΥΤ|ΝΟΤ|ΟΠΑΛ|ΟΞ|ΟΡ|ΟΣ|ΠΑΝΑΓ|ΠΑΤΡ|ΠΗΛ|ΠΗΝ|ΠΛΑΙΣ|ΠΟΝΤ|ΡΑΔ|ΡΟΔ|ΣΚ|ΣΚΟΡΠ|ΣΟΥΝ|ΣΠΑΝ|ΣΤΑΔ|ΣΥΡ|ΤΗΛ|ΤΙΜ|ΤΟΚ|ΤΟΠ|ΤΡΟΧ|ΦΙΛ|ΦΩΤ|Χ|ΧΙΛ|ΧΡΩΜ|ΧΩΡ)$").IsMatch(match[0].Groups[1].Value))
            {
                stemmedWord += "Ι";
            }
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΠΑΛ)$"))
            {
                stemmedWord += "ΑΙ";
            }
        }

        //step 4
        if ((match = new Regex("^(.+?)(ΙΚΟΣ|ΙΚΟΝ|ΙΚΕΙΣ|ΙΚΟΙ|ΙΚΕΣ|ΙΚΟΥΣ|ΙΚΗ|ΙΚΗΣ|ΙΚΟ|ΙΚΑ|ΙΚΟΥ|ΙΚΩΝ|ΙΚΩΣ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (endsInVowel(stemmedWord) || Regex.IsMatch(match[0].Groups[1].Value, "^(ΑΔ|ΑΛ|ΑΜΑΝ|ΑΜΕΡ|ΑΜΜΟΧΑΛ|ΑΝΗΘ|ΑΝΤΙΔ|ΑΠΛ|ΑΤΤ|ΑΦΡ|ΒΑΣ|ΒΡΩΜ|ΓΕΝ|ΓΕΡ|Δ|ΔΙΚΑΝ|ΔΥΤ|ΕΙΔ|ΕΝΔ|ΕΞΩΔ|ΗΘ|ΘΕΤ|ΚΑΛΛΙΝ|ΚΑΛΠ|ΚΑΤΑΔ|ΚΟΥΖΙΝ|ΚΡ|ΚΩΔ|ΛΟΓ|Μ|ΜΕΡ|ΜΟΝΑΔ|ΜΟΥΛ|ΜΟΥΣ|ΜΠΑΓΙΑΤ|ΜΠΑΝ|ΜΠΟΛ|ΜΠΟΣ|ΜΥΣΤ|Ν|ΝΙΤ|ΞΙΚ|ΟΠΤ|ΠΑΝ|ΠΕΤΣ|ΠΙΚΑΝΤ|ΠΙΤΣ|ΠΛΑΣΤ|ΠΛΙΑΤΣ|ΠΟΝΤ|ΠΟΣΤΕΛΝ|ΠΡΩΤΟΔ|ΣΕΡΤ|ΣΗΜΑΝΤ|ΣΤΑΤ|ΣΥΝΑΔ|ΣΥΝΟΜΗΛ|ΤΕΛ|ΤΕΧΝ|ΤΡΟΠ|ΤΣΑΜ|ΥΠΟΔ|Φ|ΦΙΛΟΝ|ΦΥΛΟΔ|ΦΥΣ|ΧΑΣ)$") || Regex.IsMatch(match[0].Groups[1].Value, "(ΦΟΙΝ)$"))
            {
                stemmedWord += "ΙΚ";
            }
        }

        //step 5
        //5a
        if (stemmedWord == "ΑΓΑΜΕ")
        {
            stemmedWord = "ΑΓΑΜ";
        }

        if ((match = new Regex("^(.+?)(ΑΓΑΜΕ|ΗΣΑΜΕ|ΟΥΣΑΜΕ|ΗΚΑΜΕ|ΗΘΗΚΑΜΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
        }

        if ((match = new Regex("^(.+?)(ΑΜΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΑΝΑΠ|ΑΠΟΘ|ΑΠΟΚ|ΑΠΟΣΤ|ΒΟΥΒ|ΞΕΘ|ΟΥΛ|ΠΕΘ|ΠΙΚΡ|ΠΟΤ|ΣΙΧ|Χ)$"))
            {
                stemmedWord += "ΑΜ";
            }
        }

        //5b
        if ((match = new Regex("^(.+?)(ΑΓΑΝΕ|ΗΣΑΝΕ|ΟΥΣΑΝΕ|ΙΟΝΤΑΝΕ|ΙΟΤΑΝΕ|ΙΟΥΝΤΑΝΕ|ΟΝΤΑΝΕ|ΟΤΑΝΕ|ΟΥΝΤΑΝΕ|ΗΚΑΝΕ|ΗΘΗΚΑΝΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΤΡ|ΤΣ)$"))
            {
                stemmedWord += "ΑΓΑΝ";
            }
        }
        if ((match = new Regex("^(.+?)(ΑΝΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (endsInVowel(stemmedWord) || Regex.IsMatch(match[0].Groups[1].Value, "^(ΒΕΤΕΡ|ΒΟΥΛΚ|ΒΡΑΧΜ|Γ|ΔΡΑΔΟΥΜ|Θ|ΚΑΛΠΟΥΖ|ΚΑΣΤΕΛ|ΚΟΡΜΟΡ|ΛΑΟΠΛ|ΜΩΑΜΕΘ|Μ|ΜΟΥΣΟΥΛΜΑΝ|ΟΥΛ|Π|ΠΕΛΕΚ|ΠΛ|ΠΟΛΙΣ|ΠΟΡΤΟΛ|ΣΑΡΑΚΑΤΣ|ΣΟΥΛΤ|ΤΣΑΡΛΑΤ|ΟΡΦ|ΤΣΙΓΓ|ΤΣΟΠ|ΦΩΤΟΣΤΕΦ|Χ|ΨΥΧΟΠΛ|ΑΓ|ΟΡΦ|ΓΑΛ|ΓΕΡ|ΔΕΚ|ΔΙΠΛ|ΑΜΕΡΙΚΑΝ|ΟΥΡ|ΠΙΘ|ΠΟΥΡΙΤ|Σ|ΖΩΝΤ|ΙΚ|ΚΑΣΤ|ΚΟΠ|ΛΙΧ|ΛΟΥΘΗΡ|ΜΑΙΝΤ|ΜΕΛ|ΣΙΓ|ΣΠ|ΣΤΕΓ|ΤΡΑΓ|ΤΣΑΓ|Φ|ΕΡ|ΑΔΑΠ|ΑΘΙΓΓ|ΑΜΗΧ|ΑΝΙΚ|ΑΝΟΡΓ|ΑΠΗΓ|ΑΠΙΘ|ΑΤΣΙΓΓ|ΒΑΣ|ΒΑΣΚ|ΒΑΘΥΓΑΛ|ΒΙΟΜΗΧ|ΒΡΑΧΥΚ|ΔΙΑΤ|ΔΙΑΦ|ΕΝΟΡΓ|ΘΥΣ|ΚΑΠΝΟΒΙΟΜΗΧ|ΚΑΤΑΓΑΛ|ΚΛΙΒ|ΚΟΙΛΑΡΦ|ΛΙΒ|ΜΕΓΛΟΒΙΟΜΗΧ|ΜΙΚΡΟΒΙΟΜΗΧ|ΝΤΑΒ|ΞΗΡΟΚΛΙΒ|ΟΛΙΓΟΔΑΜ|ΟΛΟΓΑΛ|ΠΕΝΤΑΡΦ|ΠΕΡΗΦ|ΠΕΡΙΤΡ|ΠΛΑΤ|ΠΟΛΥΔΑΠ|ΠΟΛΥΜΗΧ|ΣΤΕΦ|ΤΑΒ|ΤΕΤ|ΥΠΕΡΗΦ|ΥΠΟΚΟΠ|ΧΑΜΗΛΟΔΑΠ|ΨΗΛΟΤΑΒ)$"))
            {
                stemmedWord += "ΑΝ";
            }
        }

        //5c
        if ((match = new Regex("^(.+?)(ΗΣΕΤΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
        }

        if ((match = new Regex("^(.+?)(ΕΤΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (endsInVowel(stemmedWord) || Regex.IsMatch(match[0].Groups[1].Value, "(ΟΔ|ΑΙΡ|ΦΟΡ|ΤΑΘ|ΔΙΑΘ|ΣΧ|ΕΝΔ|ΕΥΡ|ΤΙΘ|ΥΠΕΡΘ|ΡΑΘ|ΕΝΘ|ΡΟΘ|ΣΘ|ΠΥΡ|ΑΙΝ|ΣΥΝΔ|ΣΥΝ|ΣΥΝΘ|ΧΩΡ|ΠΟΝ|ΒΡ|ΚΑΘ|ΕΥΘ|ΕΚΘ|ΝΕΤ|ΡΟΝ|ΑΡΚ|ΒΑΡ|ΒΟΛ|ΩΦΕΛ)$/.test(match[1])||/^(ΑΒΑΡ|ΒΕΝ|ΕΝΑΡ|ΑΒΡ|ΑΔ|ΑΘ|ΑΝ|ΑΠΛ|ΒΑΡΟΝ|ΝΤΡ|ΣΚ|ΚΟΠ|ΜΠΟΡ|ΝΙΦ|ΠΑΓ|ΠΑΡΑΚΑΛ|ΣΕΡΠ|ΣΚΕΛ|ΣΥΡΦ|ΤΟΚ|Υ|Δ|ΕΜ|ΘΑΡΡ|Θ)$"))
            {
                stemmedWord += "ΕΤ";
            }
        }

        //5d
        if ((match = new Regex("^(.+?)(ΟΝΤΑΣ|ΩΝΤΑΣ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^ΑΡΧ$"))
            {
                stemmedWord += "ΟΝΤ";
            }
            if (Regex.IsMatch(match[0].Groups[1].Value, "ΚΡΕ$"))
            {
                stemmedWord += "ΩΝΤ";
            }
        }

        //5e
        if ((match = new Regex("^(.+?)(ΟΜΑΣΤΕ|ΙΟΜΑΣΤΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^ΟΝ$"))
            {
                stemmedWord += "ΟΜΑΣΤ";
            }
        }

        //5f
        if ((match = new Regex("^(.+?)(ΙΕΣΤΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(Π|ΑΠ|ΣΥΜΠ|ΑΣΥΜΠ|ΑΚΑΤΑΠ|ΑΜΕΤΑΜΦ)$"))
            {
                stemmedWord += "ΙΕΣΤ";
            }
        }

        if ((match = new Regex("^(.+?)(ΕΣΤΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΑΛ|ΑΡ|ΕΚΤΕΛ|Ζ|Μ|Ξ|ΠΑΡΑΚΑΛ|ΑΡ|ΠΡΟ|ΝΙΣ)$"))
            {
                stemmedWord += "ΕΣΤ";
            }
        }

        //5g
        if ((match = new Regex("^(.+?)(ΗΘΗΚΑ|ΗΘΗΚΕΣ|ΗΘΗΚΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
        }

        if ((match = new Regex("^(.+?)(ΗΚΑ|ΗΚΕΣ|ΗΚΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "(ΣΚΩΛ|ΣΚΟΥΛ|ΝΑΡΘ|ΣΦ|ΟΘ|ΠΙΘ)$") || Regex.IsMatch(match[0].Groups[1].Value, "^(ΔΙΑΘ|Θ|ΠΑΡΑΚΑΤΑΘ|ΠΡΟΣΘ|ΣΥΝΘ)$"))
            {
                stemmedWord += "ΗΚ";
            }
        }

        //5h
        if ((match = new Regex("^(.+?)(ΟΥΣΑ|ΟΥΣΕΣ|ΟΥΣΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (endsInVowel(stemmedWord) || Regex.IsMatch(match[0].Groups[1].Value, "^(ΦΑΡΜΑΚ|ΧΑΔ|ΑΓΚ|ΑΝΑΡΡ|ΒΡΟΜ|ΕΚΛΙΠ|ΛΑΜΠΙΔ|ΛΕΧ|Μ|ΠΑΤ|Ρ|Λ|ΜΕΔ|ΜΕΣΑΖ|ΥΠΟΤΕΙΝ|ΑΜ|ΑΙΘ|ΑΝΗΚ|ΔΕΣΠΟΖ|ΕΝΔΙΑΦΕΡ)$") || Regex.IsMatch(match[0].Groups[1].Value, "(ΠΟΔΑΡ|ΒΛΕΠ|ΠΑΝΤΑΧ|ΦΡΥΔ|ΜΑΝΤΙΛ|ΜΑΛΛ|ΚΥΜΑΤ|ΛΑΧ|ΛΗΓ|ΦΑΓ|ΟΜ|ΠΡΩΤ)$"))
            {
                stemmedWord += "ΟΥΣ";
            }
        }

        //5i
        if ((match = new Regex("^(.+?)(ΑΓΑ|ΑΓΕΣ|ΑΓΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΑΒΑΣΤ|ΠΟΛΥΦ|ΑΔΗΦ|ΠΑΜΦ|Ρ|ΑΣΠ|ΑΦ|ΑΜΑΛ|ΑΜΑΛΛΙ|ΑΝΥΣΤ|ΑΠΕΡ|ΑΣΠΑΡ|ΑΧΑΡ|ΔΕΡΒΕΝ|ΔΡΟΣΟΠ|ΞΕΦ|ΝΕΟΠ|ΝΟΜΟΤ|ΟΛΟΠ|ΟΜΟΤ|ΠΡΟΣΤ|ΠΡΟΣΩΠΟΠ|ΣΥΜΠ|ΣΥΝΤ|Τ|ΥΠΟΤ|ΧΑΡ|ΑΕΙΠ|ΑΙΜΟΣΤ|ΑΝΥΠ|ΑΠΟΤ|ΑΡΤΙΠ|ΔΙΑΤ|ΕΝ|ΕΠΙΤ|ΚΡΟΚΑΛΟΠ|ΣΙΔΗΡΟΠ|Λ|ΝΑΥ|ΟΥΛΑΜ|ΟΥΡ|Π|ΤΡ|Μ)$") || (Regex.IsMatch(match[0].Groups[1].Value, "(ΟΦ|ΠΕΛ|ΧΟΡΤ|ΛΛ|ΣΦ|ΡΠ|ΦΡ|ΠΡ|ΛΟΧ|ΣΜΗΝ)$") && !Regex.IsMatch(match[0].Groups[1].Value, "^(ΨΟΦ|ΝΑΥΛΟΧ)$")) || Regex.IsMatch(match[0].Groups[1].Value, "(ΚΟΛΛ)$"))
            {
                stemmedWord += "ΑΓ";
            }
        }

        //5j
        if ((match = new Regex("^(.+?)(ΗΣΕ|ΗΣΟΥ|ΗΣΑ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(Ν|ΧΕΡΣΟΝ|ΔΩΔΕΚΑΝ|ΕΡΗΜΟΝ|ΜΕΓΑΛΟΝ|ΕΠΤΑΝ|Ι)$"))
            {
                stemmedWord += "ΗΣ";
            }
        }

        //5k
        if ((match = new Regex("^(.+?)(ΗΣΤΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΑΣΒ|ΣΒ|ΑΧΡ|ΧΡ|ΑΠΛ|ΑΕΙΜΝ|ΔΥΣΧΡ|ΕΥΧΡ|ΚΟΙΝΟΧΡ|ΠΑΛΙΜΨ)$"))
            {
                stemmedWord += "ΗΣΤ";
            }
        }

        //5l
        if ((match = new Regex("^(.+?)(ΟΥΝΕ|ΗΣΟΥΝΕ|ΗΘΟΥΝΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(Ν|Ρ|ΣΠΙ|ΣΤΡΑΒΟΜΟΥΤΣ|ΚΑΚΟΜΟΥΤΣ|ΕΞΩΝ)$"))
            {
                stemmedWord += "ΟΥΝ";
            }
        }

        //5m
        if ((match = new Regex("^(.+?)(ΟΥΜΕ|ΗΣΟΥΜΕ|ΗΘΟΥΜΕ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value;
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΠΑΡΑΣΟΥΣ|Φ|Χ|ΩΡΙΟΠΛ|ΑΖ|ΑΛΛΟΣΟΥΣ|ΑΣΟΥΣ)$"))
            {
                stemmedWord += "ΟΥΜ";
            }
        }

        //step 6
        //6a
        if ((match = new Regex("^(.+?)(ΜΑΤΟΙ|ΜΑΤΟΥΣ|ΜΑΤΟ|ΜΑΤΑ|ΜΑΤΩΣ|ΜΑΤΩΝ|ΜΑΤΟΣ|ΜΑΤΕΣ|ΜΑΤΗ|ΜΑΤΗΣ|ΜΑΤΟΥ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value + 'Μ';
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΓΡΑΜ)$"))
            {
                stemmedWord += "Α";
            }
            else if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΓΕ|ΣΤΑ)$"))
            {
                stemmedWord += "ΑΤ";
            }
        }

        //6b
        if ((match = new Regex("^(.+?)(ΟΥΑ)$").Matches(stemmedWord)).Count > 0)
        {
            stemmedWord = match[0].Groups[1].Value + "ΟΥ";
        }

        //Handle long words
        if (word.Length == stemmedWord.Length)
        {
            if ((match = new Regex("^(.+?)(Α|ΑΓΑΤΕ|ΑΓΑΝ|ΑΕΙ|ΑΜΑΙ|ΑΝ|ΑΣ|ΑΣΑΙ|ΑΤΑΙ|ΑΩ|Ε|ΕΙ|ΕΙΣ|ΕΙΤΕ|ΕΣΑΙ|ΕΣ|ΕΤΑΙ|Ι|ΙΕΜΑΙ|ΙΕΜΑΣΤΕ|ΙΕΤΑΙ|ΙΕΣΑΙ|ΙΕΣΑΣΤΕ|ΙΟΜΑΣΤΑΝ|ΙΟΜΟΥΝ|ΙΟΜΟΥΝΑ|ΙΟΝΤΑΝ|ΙΟΝΤΟΥΣΑΝ|ΙΟΣΑΣΤΑΝ|ΙΟΣΑΣΤΕ|ΙΟΣΟΥΝ|ΙΟΣΟΥΝΑ|ΙΟΤΑΝ|ΙΟΥΜΑ|ΙΟΥΜΑΣΤΕ|ΙΟΥΝΤΑΙ|ΙΟΥΝΤΑΝ|Η|ΗΔΕΣ|ΗΔΩΝ|ΗΘΕΙ|ΗΘΕΙΣ|ΗΘΕΙΤΕ|ΗΘΗΚΑΤΕ|ΗΘΗΚΑΝ|ΗΘΟΥΝ|ΗΘΩ|ΗΚΑΤΕ|ΗΚΑΝ|ΗΣ|ΗΣΑΝ|ΗΣΑΤΕ|ΗΣΕΙ|ΗΣΕΣ|ΗΣΟΥΝ|ΗΣΩ|Ο|ΟΙ|ΟΜΑΙ|ΟΜΑΣΤΑΝ|ΟΜΟΥΝ|ΟΜΟΥΝΑ|ΟΝΤΑΙ|ΟΝΤΑΝ|ΟΝΤΟΥΣΑΝ|ΟΣ|ΟΣΑΣΤΑΝ|ΟΣΑΣΤΕ|ΟΣΟΥΝ|ΟΣΟΥΝΑ|ΟΤΑΝ|ΟΥ|ΟΥΜΑΙ|ΟΥΜΑΣΤΕ|ΟΥΝ|ΟΥΝΤΑΙ|ΟΥΝΤΑΝ|ΟΥΣ|ΟΥΣΑΝ|ΟΥΣΑΤΕ|Υ||ΥΑ|ΥΣ|Ω|ΩΝ|ΟΙΣ)$").Matches(stemmedWord)).Count > 0)
            {
                stemmedWord = match[0].Groups[1].Value;
            }
        }

        //step 7
        if ((match = new Regex("^(.+?)(ΕΣΤΕΡ|ΕΣΤΑΤ|ΟΤΕΡ|ΟΤΑΤ|ΥΤΕΡ|ΥΤΑΤ|ΩΤΕΡ|ΩΤΑΤ)$").Matches(stemmedWord)).Count > 0)
        {
            if (!Regex.IsMatch(match[0].Groups[1].Value, "^(ΕΞ|ΕΣ|ΑΝ|ΚΑΤ|Κ|ΠΡ)$"))
            {
                stemmedWord = match[0].Groups[1].Value;
            }
            if (Regex.IsMatch(match[0].Groups[1].Value, "^(ΚΑ|Μ|ΕΛΕ|ΛΕ|ΔΕ)$"))
            {
                stemmedWord += "ΥΤ";
            }
        }

        return stemmedWord;
    }

    public bool isGreek(string word)
    {
        return alphabet.IsMatch(word);
    }

    public bool endsInVowel(string word)
    {
        return new Regex("[ΑΕΗΙΟΥΩ]$").IsMatch(word);
    }

    static string ConvertAndNormalize(string text)
    {

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
        byte[] unicodeBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8Bytes);
        text = Encoding.Unicode.GetString(unicodeBytes);

        //normalize to remove diacritics
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).ToUpper();
    }
}

public class StemmerConfig
{
    public Dictionary<string, string> exceptions { get; set; }
    public string[] protectedwords { get; set; }
}