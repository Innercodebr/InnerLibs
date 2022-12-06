﻿using InnerLibs.LINQ;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;

namespace InnerLibs.Locations
{
    public static class AddressTypes
    {
        #region Public Properties

        public static string[] Aeroporto => new[] { "Aeroporto", "Ar", "Aero", "Air" };
        public static string[] Alameda => new[] { "Alameda", "Al", "Alm" };
        public static string[] Área => new[] { "Área", "Area" };
        public static string[] Avenida => new[] { "Avenida", "Av", "Avn" };
        public static string[] Campo => new[] { "Cam", "Camp", "Campo" };
        public static string[] Chácara => new[] { "Cha", "Chac", "Chacara" };
        public static string[] Colônia => new[] { "Col", "Colonia" };
        public static string[] Comunidade => new[] { "Com", "Comunidade" };
        public static string[] Condomínio => new[] { "Condominio", "Cond" };
        public static string[] Conjunto => new[] { "Conjunto", "Con" };
        public static string[] Distrito => new[] { "Dis", "Dst", "Distrito" };
        public static string[] Esplanada => new[] { "Esp", "Esplanada" };
        public static string[] Estação => new[] { "Estacao", "Est", "St" };
        public static string[] Estrada => new[] { "Et", "Es", "Estrada" };
        public static string[] Favela => new[] { "Fav", "Favela" };
        public static string[] Feira => new[] { "Fei", "Fr", "Feira" };
        public static string[] Jardim => new[] { "Jd", "Jardim", "Jar" };
        public static string[] Ladeira => new[] { "Lad", "Ld", "Ladeira" };
        public static string[] Lago => new[] { "Lago", "Lg" };
        public static string[] Lagoa => new[] { "Lagoa", "Lga" };
        public static string[] Largo => new[] { "Lrg", "Largo", "Lgo" };
        public static string[] Loteamento => new[] { "Lote", "Loteamento", "Lt" };
        public static string[] Morro => new[] { "Mr", "Mrr", "Morro", "Mor" };
        public static string[] Núcleo => new[] { "Nc", "Nucleo", "Nuc" };
        public static string[] Parque => new[] { "Parque", "Pq", "Pk", "Par", "Parq", "Park" };
        public static string[] Passarela => new[] { "Pass", "Pas", "Pa", "Passarela" };
        public static string[] Pátio => new[] { "Pt", "Pat", "Pateo", "Patio" };
        public static string[] Praça => new[] { "Praça", "Pc", "Praca", "Pç" };
        public static string[] Quadra => new[] { "Qd", "Quadra", "Quad" };
        public static string[] Recanto => new[] { "Rec", "Recanto", "Rc" };
        public static string[] Residencial => new[] { "Residencial", "Residencia", "Res", "Resid" };
        public static string[] Rodovia => new[] { "Rodovia", "Rod" };
        public static string[] Rua => new[] { "Rua", "R" };
        public static string[] Setor => new[] { "Setor", "Str" };
        public static string[] Sítio => new[] { "Sitio", "Sit" };
        public static string[] Travessa => new[] { "Trv", "Travessa", "Tvs" };
        public static string[] Trecho => new[] { "Trc", "Trecho" };
        public static string[] Trevo => new[] { "Trevo" };
        public static string[] Vale => new[] { "Vale", "Val" };
        public static string[] Vereda => new[] { "Vereda" };
        public static string[] Via => new[] { "Via" };
        public static string[] Viaduto => new[] { "Vd", "Viaduto" };
        public static string[] Viela => new[] { "Viela" };
        public static string[] Vila => new[] { "Vila", "Vl" };

        #endregion Public Properties

        #region Public Methods

        public static string GetAddressType(string Endereco) => GetAddressTypeProperty(Endereco)?.Name.IfBlank(InnerLibs.Text.Empty);

        public static string[] GetAddressTypeList(string Endereco) => (string[])(GetAddressTypeProperty(Endereco)?.GetValue(null) ?? new string[] { });

        public static PropertyInfo GetAddressTypeProperty(string Endereco)
        {
            string tp = Endereco.IfBlank(InnerLibs.Text.Empty).Split(PredefinedArrays.WordSplitters.ToArray(), StringSplitOptions.RemoveEmptyEntries).FirstOr(InnerLibs.Text.Empty);
            if (tp.IsNotBlank())
            {
                var df = typeof(AddressTypes);
                return df.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).FirstOrDefault(x => x.Name == tp || Misc.IsIn(tp, (string[])x.GetValue(null)));
            }

            return null;
        }

        #endregion Public Methods
    }

    /// <summary>
    /// Representa um deteminado local com suas Informações
    /// </summary>
    /// <remarks></remarks>
    public class AddressInfo
    {
        #region Private Fields

        private static AddressPart _globalformat = AddressPart.FullAddress;

        private AddressPart _format = AddressPart.Default;

        private Dictionary<string, string> details = new Dictionary<string, string>();

        #endregion Private Fields

        #region Private Methods

        private static string PropCleaner(string value) => value.IfBlank(InnerLibs.Text.Empty).TrimAny(true, " ", ".", " ", ",", " ", "-", " ").ToTitle().NullIf(x => x.IsBlank());

        #endregion Private Methods

        #region Public Constructors

        /// <summary>
        /// Cria um novo objeto de localização
        /// </summary>
        public AddressInfo()
        {
        }

        #endregion Public Constructors

        #region Public Indexers

        public string this[string key]
        {
            get
            {
                if (details is null)
                {
                    details = new Dictionary<string, string>();
                }

                if (key.IsBlank())
                {
                    return InnerLibs.Text.Empty;
                }

                key = key.ToLowerInvariant();
                if (!details.ContainsKey(key))
                {
                    switch (key)
                    {
                        case "geolocation":
                            {
                                return LatitudeLongitude();
                            }

                        case "fulladdress":
                        case "tostring":
                        case "address":
                            {
                                return FullAddress;
                            }

                        case "streetname":
                        case "name":
                            {
                                return Name;
                            }

                        case "streettype":
                        case "type":
                            {
                                return Type;
                            }

                        default:
                            {
                                break;
                            }
                    }
                }

                return details.GetValueOr(key, null);
            }

            set
            {
                if (key.IsNotBlank())
                {
                    if (value.IsNotBlank())
                    {
                        details[key.ToLowerInvariant()] = value.TrimAny(" ");
                    }
                    else if (ContainsKey(key))
                    {
                        Remove(key);
                    }
                }
            }
        }

        #endregion Public Indexers

        #region Public Properties

        /// <summary>
        /// Formato global de todas as intancias de <see cref="AddressInfo"/> quando chamadas pelo
        /// <see cref="AddressInfo.ToString()"/>
        /// </summary>
        /// <returns></returns>
        public static AddressPart GlobalFormat
        {
            get
            {
                if (_globalformat == AddressPart.Default)
                {
                    return AddressPart.FullAddress;
                }

                return _globalformat;
            }

            set
            {
                _globalformat = value;
            }
        }

        public string City
        {
            get => this[nameof(City)];
            set => this[nameof(City)] = PropCleaner(value);
        }

        public string Complement
        {
            get => this[nameof(Complement)];
            set => this[nameof(Complement)] = PropCleaner(value);
        }

        public string Country
        {
            get => this[nameof(Country)];
            set => this[nameof(Country)] = PropCleaner(value);
        }

        public string CountryCode
        {
            get => this[nameof(CountryCode)];
            set => this[nameof(CountryCode)] = PropCleaner(value)?.ToUpper();
        }

        /// <summary>
        /// Formato desta instancia de <see cref="AddressInfo"/> quando chamada pelo <see cref="ToString()"/>
        /// </summary>
        /// <returns></returns>
        public AddressPart Format
        {
            get
            {
                if (_format == AddressPart.Default)
                {
                    return GlobalFormat;
                }

                return _format;
            }

            set
            {
                _format = value;
            }
        }

        /// <summary>
        /// Retorna o endereço completo
        /// </summary>
        /// <returns>Uma String com o endereço completo devidamente formatado</returns>
        public string FullAddress => ToString(AddressPart.FullAddress);

        /// <summary>
        /// Logradouro, Numero e Complemento
        /// </summary>
        /// <returns>Uma String com o endereço completo devidamente formatado</returns>
        public string FullLocationInfo => ToString(AddressPart.FullLocationInfo);

        public decimal? Latitude
        {
            get
            {
                string value = this[nameof(Latitude)];
                if (value != null)
                {
                    return Convert.ToDecimal(value, new CultureInfo("en-US"));
                }

                return default;
            }

            set
            {
                if (value.HasValue)
                {
                    this[nameof(Latitude)] = Convert.ToString(value.Value, new CultureInfo("en-US"));
                }
                else
                {
                    Remove(nameof(Latitude));
                }
            }
        }

        /// <summary>
        /// Retorna o Logradouro e numero
        /// </summary>
        /// <returns>Uma String com o endereço completo devidamente formatado</returns>
        public string LocationInfo => ToString(AddressPart.LocationInfo);

        /// <summary>
        /// Coordenada geográfica LONGITUDE
        /// </summary>
        /// <value></value>
        /// <returns>Longitude</returns>
        public decimal? Longitude
        {
            get
            {
                string value = this[nameof(Longitude)];
                if (value != null)
                {
                    return Convert.ToDecimal(value, new CultureInfo("en-US"));
                }

                return default;
            }

            set
            {
                if (value.HasValue)
                {
                    this[nameof(Longitude)] = Convert.ToString(value, new CultureInfo("en-US"));
                }
                else
                {
                    Remove(nameof(Longitude));
                }
            }
        }

        /// <summary>
        /// O nome do endereço
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public virtual string Name => Street.TrimAny(AddressTypes.GetAddressTypeList(Street)).TrimAny(" ");

        public string Neighborhood
        {
            get => this[nameof(Neighborhood)];
            set => this[nameof(Neighborhood)] = PropCleaner(value);
        }

        public string Number
        {
            get => this[nameof(Number)];
            set => this[nameof(Number)] = value;
        }

        public string PostalCode
        {
            get => this[nameof(PostalCode)];
            set => this[nameof(PostalCode)] = FormatPostalCode(PropCleaner(value));
        }

        public string Region
        {
            get => this[nameof(Region)];
            set => this[nameof(Region)] = PropCleaner(value);
        }

        public string State
        {
            get => this[nameof(State)];
            set => this[nameof(State)] = PropCleaner(value);
        }

        public string StateCode
        {
            get => this[nameof(StateCode)];
            set => this[nameof(StateCode)] = PropCleaner(value)?.ToUpper();
        }

        /// <summary>
        /// Logradouro
        /// </summary>
        /// <returns></returns>
        public string Street
        {
            get => this[nameof(Street)];

            set
            {
                if (value.IsNotBlank())
                {
                    this[nameof(Street)] = PropCleaner($"{AddressTypes.GetAddressType(value)} {value.TrimAny(true, AddressTypes.GetAddressTypeList(value))}").TrimBetween();
                }
                else
                {
                    this[nameof(Street)] = null;
                }
            }
        }

        /// <summary>
        /// Tipo do Endereço
        /// </summary>
        /// <returns></returns>
        public string Type => AddressTypes.GetAddressType(Street);

        /// <summary>
        /// CEP - Codigo de Endereçamento Postal. Alias de <see cref="PostalCode"/>
        /// </summary>
        /// <value></value>
        /// <returns>CEP</returns>
        public string ZipCode
        {
            get => PostalCode;

            set => PostalCode = value;
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Cria uma localização a partir de partes de endereço
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Number"></param>
        /// <param name="Complement"></param>
        /// <param name="Neighborhood"></param>
        /// <param name="City"></param>
        /// <param name="State"></param>
        /// <param name="Country"></param>
        /// <param name="PostalCode"></param>
        /// <returns></returns>
        public static AddressInfo CreateLocation(string Address, string Number = InnerLibs.Text.Empty, string Complement = InnerLibs.Text.Empty, string Neighborhood = InnerLibs.Text.Empty, string City = InnerLibs.Text.Empty, string State = InnerLibs.Text.Empty, string Country = InnerLibs.Text.Empty, string PostalCode = InnerLibs.Text.Empty) => CreateLocation<AddressInfo>(Address, Number, Complement, Neighborhood, City, State, Country, PostalCode);

        /// <summary>
        /// Cria uma localização a partir de partes de endereço
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Number"></param>
        /// <param name="Complement"></param>
        /// <param name="Neighborhood"></param>
        /// <param name="City"></param>
        /// <param name="State"></param>
        /// <param name="Country"></param>
        /// <param name="PostalCode"></param>
        /// <returns></returns>
        public static T CreateLocation<T>(string Address, string Number = InnerLibs.Text.Empty, string Complement = InnerLibs.Text.Empty, string Neighborhood = InnerLibs.Text.Empty, string City = InnerLibs.Text.Empty, string State = InnerLibs.Text.Empty, string Country = InnerLibs.Text.Empty, string PostalCode = InnerLibs.Text.Empty) where T : AddressInfo
        {
            var l = Activator.CreateInstance<T>();
            l.Street = Address;
            l.Neighborhood = Neighborhood;
            l.Complement = Complement;
            l.Number = Number;
            l.City = City;
            l.PostalCode = PostalCode;
            var st = Brasil.GetState(State);
            if (st != null)
            {
                l.State = st.Name;
                l.StateCode = st.StateCode;
                l.Region = st.Region;
                l.Country = "Brasil";
                l.CountryCode = "BR";
            }
            else
            {
                if ((State?.Length) == 2)
                {
                    l.StateCode = State;
                }
                else
                {
                    l.State = State;
                }

                if ((Country?.Length) == 2)
                {
                    l.CountryCode = Country.ToUpper();
                }
                else
                {
                    l.Country = Country;
                }
            }

            return l;
        }

        /// <summary>
        /// Retorna uma string de endereço a partir de varias partes de endereco
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Number"></param>
        /// <param name="Complement"></param>
        /// <param name="Neighborhood"></param>
        /// <param name="City"></param>
        /// <param name="State"></param>
        /// <param name="Country"></param>
        /// <param name="PostalCode"></param>
        /// <returns></returns>
        public static string FormatAddress(string Address = null, string Number = InnerLibs.Text.Empty, string Complement = InnerLibs.Text.Empty, string Neighborhood = InnerLibs.Text.Empty, string City = InnerLibs.Text.Empty, string State = InnerLibs.Text.Empty, string Country = InnerLibs.Text.Empty, string PostalCode = InnerLibs.Text.Empty, params AddressPart[] Parts) => CreateLocation(Address, Number, Complement, Neighborhood, City, State, Country, PostalCode).ToString(Parts);

        public static string FormatPostalCode(string CEP)
        {
            CEP = CEP.IfBlank(InnerLibs.Text.Empty).Trim();
            if (CEP.IsValidCEP())
            {
                CEP = CEP.FormatCEP();
            }

            return CEP;
        }

        /// <summary>
        /// Retorna um <see cref="AddressInfo"/> usando a api de geocode do Google Maps para
        /// completar as informações
        /// </summary>
        /// <param name="Address">Endereço para Busca</param>
        /// <param name="Key">Chave de acesso da API</param>
        /// <returns></returns>
        public static AddressInfo FromGoogleMaps(string Address, string Key, NameValueCollection NVC = null) => FromGoogleMaps<AddressInfo>(Address, Key, NVC);

        /// <summary>
        /// Retorna um <see cref="AddressInfo"/> usando a api de geocode do Google Maps para
        /// completar as informações
        /// </summary>
        /// <param name="Address">Endereço para Busca</param>
        /// <param name="Key">Chave de acesso da API</param>
        /// <returns></returns>
        public static T FromGoogleMaps<T>(string Address, string Key, NameValueCollection NVC = null) where T : AddressInfo
        {
            var d = Activator.CreateInstance<T>();
            NVC = NVC ?? new NameValueCollection();
            NVC["key"] = Key;
            NVC["address"] = Address;
            d["original_string"] = Address;
            string url = $"https://maps.googleapis.com/maps/api/geocode/xml?{NVC.ToQueryString()}";
            d["search_url"] = url;
            using (var c = new WebClient())
            {
                var xml = new XmlDocument();
                xml.LoadXml(c.DownloadString(url));
                var x = xml["GeocodeResponse"];
                string status = x["status"].InnerText;
                d["status"] = status;
                if (status == "OK")
                {
                    foreach (XmlNode item in x["result"].ChildNodes)
                    {
                        if (item.Name == "geometry")
                        {
                            var cultura = new CultureInfo("en-US");
                            decimal lat = Convert.ToDecimal(item["location"]["lat"].InnerText, cultura);
                            decimal lng = Convert.ToDecimal(item["location"]["lng"].InnerText, cultura);
                            d.Latitude = lat;
                            d.Longitude = lng;
                        }

                        if (item.Name == "place_id")
                        {
                            d["place_id"] = item.InnerText;
                        }

                        if (item.Name == "address_component")
                        {
                            foreach (XmlNode subitem in item.ChildNodes)
                            {
                                if (subitem.Name == "type")
                                {
                                    d[subitem.InnerText] = item["long_name"].InnerText;
                                    switch (subitem.InnerText ?? InnerLibs.Text.Empty)
                                    {
                                        case "postal_code":
                                            {
                                                d.PostalCode = item["long_name"].InnerText;
                                                break;
                                            }

                                        case "country":
                                            {
                                                d.Country = item["long_name"].InnerText;
                                                d.CountryCode = item["short_name"].InnerText;
                                                break;
                                            }

                                        case "administrative_area_level_1":
                                            {
                                                d.State = item["long_name"].InnerText;
                                                d.StateCode = item["short_name"].InnerText;
                                                d.Region = Brasil.GetRegionOf(d.StateCode);
                                                break;
                                            }

                                        case "administrative_area_level_2":
                                            {
                                                d.City = item["long_name"].InnerText;
                                                break;
                                            }

                                        case "administrative_area_level_3":
                                        case "locality":
                                        case "sublocality":
                                            {
                                                d.Neighborhood = d.Neighborhood.IfBlank(item["long_name"].InnerText);
                                                break;
                                            }

                                        case "route":
                                            {
                                                d.Street = item["long_name"].InnerText;
                                                break;
                                            }

                                        case "street_number":
                                            {
                                                d.Number = item["long_name"].InnerText;
                                                break;
                                            }

                                        default:
                                            {
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (x["error_message"] != null)
                {
                    d["error_message"] = x["error_message"].InnerText;
                }
            }

            return d;
        }

        /// <summary>
        /// Cria um objeto de localização e imadiatamente pesquisa as informações de um local
        /// através do CEP usando as APIs ViaCEP
        /// </summary>
        /// <param name="PostalCode"></param>
        /// <param name="Number">Numero da casa</param>
        public static AddressInfo FromViaCEP(int PostalCode, string Number = null, string Complement = null) => FromViaCEP<AddressInfo>(PostalCode, Number, Complement);

        /// <summary>
        /// Cria um objeto de localização e imadiatamente pesquisa as informações de um local
        /// através do CEP usando as APIs ViaCEP
        /// </summary>
        /// <param name="PostalCode"></param>
        /// <param name="Number">Numero da casa</param>
        public static AddressInfo FromViaCEP(string PostalCode, string Number = null, string Complement = null) => FromViaCEP<AddressInfo>(PostalCode, Number, Complement);

        /// <summary>
        /// Cria um objeto de localização e imadiatamente pesquisa as informações de um local
        /// através do CEP usando as APIs ViaCEP
        /// </summary>
        /// <param name="PostalCode"></param>
        /// <param name="Number">Numero da casa</param>
        public static T FromViaCEP<T>(int PostalCode, string Number = null, string Complement = null) where T : AddressInfo => FromViaCEP<T>(PostalCode.ToString().PadLeft(8, '0'), Number, Complement);

        /// <summary>
        /// Cria um objeto de localização e imadiatamente pesquisa as informações de um local
        /// através do CEP usando as APIs ViaCEP
        /// </summary>
        /// <param name="PostalCode"></param>
        /// <param name="Number">Numero da casa</param>
        public static T FromViaCEP<T>(string PostalCode, string Number = null, string Complement = null) where T : AddressInfo
        {
            var d = Activator.CreateInstance<T>();
            d["original_string"] = PostalCode;
            d.PostalCode = PostalCode;
            if (Number.IsNotBlank())
            {
                d.Number = Number;
            }

            if (Complement.IsNotBlank())
            {
                d.Complement = Complement;
            }

            try
            {
                string url = "https://viacep.com.br/ws/" + d.PostalCode.RemoveAny("-") + "/xml/";
                d["search_url"] = url;
                using (var c = new WebClient())
                {
                    var x = new XmlDocument();
                    x.LoadXml(c.DownloadString(url));
                    var cep = x["xmlcep"];
                    d.Neighborhood = cep["bairro"]?.InnerText;
                    d.City = cep["localidade"]?.InnerText;
                    d.StateCode = cep["uf"]?.InnerText;
                    d.State = Brasil.GetNameOf(d.StateCode);
                    d.Region = Brasil.GetRegionOf(d.StateCode);
                    d.Street = cep["logradouro"]?.InnerText;

                    Misc.TryExecute(() => d["DDD"] = cep["ddd"]?.InnerText);
                    Misc.TryExecute(() => d["IBGE"] = cep["ibge"]?.InnerText);
                    Misc.TryExecute(() => d["GIA"] = cep["gia"]?.InnerText);
                    Misc.TryExecute(() => d["SIAFI"] = cep["SIAFI"]?.InnerText);

                    d.Country = "Brasil";
                    d.CountryCode = "BR";
                }
            }
            catch
            {
            }

            return d;
        }

        public static implicit operator AddressInfo(Point Point) => new AddressInfo().SetLatitudeLongitudeFromPoint(Point);

        public static implicit operator Point(AddressInfo AddressInfo) => (Point)AddressInfo?.GetPoint();

        /// <summary>
        /// Tenta extrair as partes de um endereço de uma string
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public static AddressInfo TryParse(string Address) => TryParse<AddressInfo>(Address);

        /// <summary>
        /// Tenta extrair as partes de um endereço de uma string
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public static T TryParse<T>(string Address) where T : AddressInfo
        {
            string original = Address;
            string PostalCode = InnerLibs.Text.Empty;
            string State = InnerLibs.Text.Empty;
            string City = InnerLibs.Text.Empty;
            string Neighborhood = InnerLibs.Text.Empty;
            string Complement = InnerLibs.Text.Empty;
            string Number = InnerLibs.Text.Empty;
            string Country = InnerLibs.Text.Empty;
            Address = Address.FixText().TrimLastAny("."); // arruma os espacos do endereco
            var ceps = Address.FindCEP(); // procura ceps no endereco
            Address = Address.RemoveAny(ceps); // remove ceps
            Address = Address.FixText().TrimLastAny("."); // arruma os espacos do endereco
            if (ceps.Any())
            {
                PostalCode = FormatPostalCode(ceps.First());
            }

            Address = Address.FixText().TrimAny("-", ",", "/"); // arruma os espacos do endereco
            if (Address.Contains(" - "))
            {
                var parts = Address.Split(" - ").ToList();
                if (parts.Count == 1)
                {
                    Address = parts.First().IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",", "-");
                }

                if (parts.Count == 2)
                {
                    Address = parts.First().IfBlank(InnerLibs.Text.Empty);
                    string maybe_neigh = parts.Last().IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",", "-");
                    if (maybe_neigh.Length == 2)
                    {
                        State = maybe_neigh;
                    }
                    else
                    {
                        Neighborhood = maybe_neigh;
                    }
                }

                if (parts.Count == 3)
                {
                    Address = parts.First().IfBlank(InnerLibs.Text.Empty);
                    string maybe_city = parts.Last().IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",", "-");
                    if (maybe_city.Length == 2)
                    {
                        State = maybe_city;
                    }
                    else
                    {
                        City = maybe_city;
                    }

                    parts.RemoveLast();
                    parts = parts.Skip(1).ToList();
                    Neighborhood = parts.FirstOrDefault().IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",", "-");
                }

                if (parts.Count == 6)
                {
                    string ad = parts[0] + ", " + parts[1] + " " + parts[2];
                    parts.RemoveAt(1);
                    parts.RemoveAt(2);
                    parts[0] = ad;
                }

                if (parts.Count == 5)
                {
                    string ad = parts[0] + ", " + parts[1];
                    parts.RemoveAt(1);
                    parts[0] = ad;
                }

                if (parts.Count == 4)
                {
                    Address = parts.First().IfBlank(InnerLibs.Text.Empty);
                    string maybe_state = parts.Last().IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",", "-");
                    parts.RemoveLast();
                    string maybe_city = parts.Last().IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",", "-");
                    parts.RemoveLast();
                    City = maybe_city;
                    State = maybe_state;
                    parts = parts.Skip(1).ToList();
                    Neighborhood = parts.FirstOrDefault().IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",", "-");
                }
            }

            Address = Address.FixText();
            if (Address.Contains(","))
            {
                var parts = Address.GetAfter(",").SplitAny(" ", ".", ",").ToList();
                Number = parts.FirstOrDefault(x => x == "s/n" || x == "sn" || x == "s" || x == "sem" || x.IsNumber());
                parts.Remove(Number);
                Complement = parts.SelectJoinString(" ");
                Address = Address.GetBefore(",");
            }
            else
            {
                var adparts = Address.SplitAny(" ", "-").ToList();
                if (adparts.Any())
                {
                    string maybe_number = adparts.FirstOrDefault(x => x == "s/n" || x == "sn" || x.IsNumber()).IfBlank(InnerLibs.Text.Empty).TrimAny(" ", ",");
                    Complement = adparts.SelectJoinString(" ").GetAfter(maybe_number).TrimAny(" ", ",");
                    Number = maybe_number;
                    Address = adparts.SelectJoinString(" ").GetBefore(maybe_number).TrimAny(" ", ",");
                }
            }

            Number = Number.TrimBetween().TrimAny(" ", ",", "-");
            Complement = Complement.TrimBetween().TrimAny(" ", ",", "-");
            var d = CreateLocation<T>(Address, Number, Complement, Neighborhood, City, State, Country, PostalCode);
            d["original_string"] = original;
            return d;
        }

        public void Add(string key, string value) => this[key] = value;

        public void Clear() => details.Clear();

        public bool ContainsKey(string key) => details.ContainsKey(key.ToLowerInvariant());

        /// <summary>
        /// Retona uma informação deste endereço
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetDetails() => details.ToDictionary();

        public Point GetPoint() => Latitude.HasValue && Longitude.HasValue ? new Point((Longitude * 1000000).ToInt(), (Latitude * 1000000).ToInt()) : new Point();

        public string LatitudeLongitude() => Latitude.HasValue && Longitude.HasValue ? $"{Latitude?.ToString(new CultureInfo("en-US"))}, {Longitude?.ToString(new CultureInfo("en-US"))}" : null;

        public bool Remove(string key)
        {
            if (ContainsKey(key))
            {
                details.Remove(key);
                return true;
            }

            return false;
        }

        public AddressInfo SetLatitudeLongitudeFromPoint(Point Point)
        {
            Longitude = Point.X * 0.000001m;
            Latitude = Point.Y * 0.000001m;
            return this;
        }

        /// <summary>
        /// Retorna uma String contendo as informações do Local
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() => ToString(Format);

        /// <summary>
        /// Retorna uma string com as partes dos endereço especificas
        /// </summary>
        /// <param name="Parts"></param>
        /// <returns></returns>
        public string ToString(IEnumerable<AddressPart> Parts) => ToString(Parts.ToArray());

        /// <summary>
        /// Retorna uma string com as partes dos endereço especificas
        /// </summary>
        /// <param name="Parts"></param>
        /// <returns></returns>
        public string ToString(params AddressPart[] Parts)
        {
            string retorno = InnerLibs.Text.Empty;

            Parts = Parts ?? new[] { Format };
            AddressPart p = Parts.IfNoIndex(0, Format);
            Parts = Parts.Skip(1).ToArray();
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] < 0)
                {
                    p &= Parts[i];
                }
                if (Parts[i] > 0)
                {
                    p |= Parts[i];
                }
            }

            if (p == 0) p = Format;

            if (p < 0)
            {
                p = Format & p;
            }

            retorno = retorno
            .AppendIf(Type, Type.IsNotBlank() && p.HasFlag(AddressPart.StreetType))
            .AppendIf($" {Name}", Name.IsNotBlank() && p.HasFlag(AddressPart.StreetName))
            .AppendIf($", {Number}", Number.IsNotBlank() && p.HasFlag(AddressPart.Number))
            .AppendIf($", {Complement}", Complement.IsNotBlank() && p.HasFlag(AddressPart.Complement))
            .AppendIf($" - {Neighborhood}", Neighborhood.IsNotBlank() && p.HasFlag(AddressPart.Neighborhood))
            .AppendIf($" - {City}", City.IsNotBlank() && p.HasFlag(AddressPart.City));

            if (p.HasFlag(AddressPart.State) && State.IsNotBlank())
            {
                retorno = retorno.AppendIf($" - {State}", State.IsNotBlank() && p.HasFlag(AddressPart.State));
            }
            else
            {
                retorno = retorno.AppendIf($" - {StateCode}", StateCode.IsNotBlank() && p.HasFlag(AddressPart.StateCode));
            }

            retorno = retorno.AppendIf($" - {PostalCode}", PostalCode.IsNotBlank() && p.HasFlag(AddressPart.PostalCode));

            if (p.HasFlag(AddressPart.Country) && Country.IsNotBlank())
            {
                retorno = retorno.AppendIf($" - {Country}", Country.IsNotBlank() && p.HasFlag(AddressPart.Country));
            }
            else
            {
                retorno = retorno.AppendIf($" - {CountryCode}", CountryCode.IsNotBlank() && p.HasFlag(AddressPart.CountryCode));
            }

            return retorno.TrimBetween().TrimAny(".", " ", ",", " ", "-");
        }

        #endregion Public Methods
    }

    /// <summary>
    /// Partes de um Endereço
    /// </summary>
    [Flags]
    public enum AddressPart
    {
        /// <summary>
        /// Formato default definido pela propriedade <see cref="AddressInfo.Format"/> ou <see cref="AddressInfo.GlobalFormat"/>
        /// </summary>
        Default = 0,

        /// <summary>
        /// Tipo do Lograoduro
        /// </summary>
        StreetType = 1,

        /// <summary>
        /// Nome do Logradouro
        /// </summary>
        StreetName = 2,

        /// <summary>
        /// Logradouro
        /// </summary>
        Street = StreetType + StreetName,

        /// <summary>
        /// Numero do local
        /// </summary>
        Number = 4,

        /// <summary>
        /// Complemento do local
        /// </summary>
        Complement = 8,

        /// <summary>
        /// Numero e complemento
        /// </summary>
        LocationInfo = Number + Complement,

        /// <summary>
        /// Logradouro, Numero e complemento
        /// </summary>
        FullLocationInfo = Street + Number + Complement,

        /// <summary>
        /// Bairro
        /// </summary>
        Neighborhood = 16,

        /// <summary>
        /// Cidade
        /// </summary>
        City = 32,

        /// <summary>
        /// Estado
        /// </summary>
        State = 64,

        /// <summary>
        /// Cidade e Estado
        /// </summary>
        CityState = City + State,

        /// <summary>
        /// UF
        /// </summary>
        StateCode = 128,

        /// <summary>
        /// Cidade e UF
        /// </summary>
        CityStateCode = City + StateCode,

        /// <summary>
        /// País
        /// </summary>
        Country = 256,

        /// <summary>
        /// País
        /// </summary>
        CountryCode = 512,

        /// <summary>
        /// CEP
        /// </summary>
        PostalCode = 1024,

        /// <summary>
        /// Endereço completo
        /// </summary>
        FullAddress = Street + LocationInfo + Neighborhood + CityStateCode + Country + PostalCode
    }
}