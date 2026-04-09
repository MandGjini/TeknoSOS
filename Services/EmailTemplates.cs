public static class EmailTemplates
{
    public static string WelcomeEmail(User user) =>
$@"<h2>Mirë se vini në TeknoSOS!</h2>\n<p>Përshëndetje {user.Name},</p>\n<p>Faleminderit që u regjistruat në platformën tonë.</p>";

    public static string TechnicianCertificateRequest(Technician tech) =>
$@"<h2>Ngarko certifikatat</h2>\n<p>Përshëndetje {tech.Name},</p>\n<p>Ju lutem ngarkoni certifikatat tuaja për verifikim në TeknoSOS.</p>";

    public static string TechnicianActivated(Technician tech) =>
$@"<h2>Llogaria u aktivizua</h2>\n<p>Përshëndetje {tech.Name},</p>\n<p>Llogaria juaj si teknik është verifikuar dhe aktivizuar.</p>";

    public static string TechnicianDeactivated(Technician tech) =>
$@"<h2>Llogaria u çaktivizua</h2>\n<p>Përshëndetje {tech.Name},</p>\n<p>Llogaria juaj është çaktivizuar për shkak të mos‑pagesës ose arsyeve të tjera.</p>";

    public static string SubscriptionExpired(User user) =>
$@"<h2>Abonimi ka skaduar</h2>\n<p>Përshëndetje {user.Name},</p>\n<p>Abonimi juaj në TeknoSOS ka përfunduar. Ju lutem rinovoni për të vazhduar shërbimet.</p>";
    public static string DefectCreated(Defect defect) =>
        $@"<h2>Defekt i ri</h2>\n<p>{defect.Title}</p>\n<p>{defect.Description}</p>";

    public static string ActionNotification(string actionType, object data) =>
        $@"<h2>Njoftim: {actionType}</h2>\n<p>{data}</p>";
}
