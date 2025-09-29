var express = require("express");
var router = express.Router();

/**
 * @swagger
 * /seguro-privado:
 *   get:
 *     summary: Calcula las deducciones mensuales según edad y dependientes
 *     parameters:
 *       - in: query
 *         name: edad
 *         required: true
 *         schema:
 *           type: integer
 *           minimum: 0
 *       - in: query
 *         name: dependientes
 *         required: true
 *         schema:
 *           type: integer
 *           minimum: 0
 *       - in: query
 *         name: token
 *         required: false
 *         schema:
 *           type: string
 *     responses:
 *       200:
 *         description: Deducciones calculadas correctamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 deductions:
 *                   type: array
 *                   items:
 *                     type: object
 *                     properties:
 *                       type:
 *                         type: string
 *                       Amount:
 *                         type: number
 *       400:
 *         description: Parámetros inválidos
 *       403:
 *         description: Token inválido o ausente
 */

router.get("/seguro-privado", function (req, res) {
  const { edad, dependientes } = req.query;

  if (!edad || !dependientes) {
    return res.status(400).json({ error: "Faltan parámetros" });
  }

  const edadNum = Number(edad);
  const depNum = Number(dependientes);

  if (isNaN(edadNum) || edadNum < 0) {
    return res.status(400).json({ error: "Edad inválida" });
  }
  if (isNaN(depNum) || depNum < 0) {
    return res.status(400).json({ error: "Dependientes inválidos" });
  }

  // Prima base según edad
  let erAmount = 0;
  if (edadNum >= 18 && edadNum <= 35) erAmount = 100000;
  else if (edadNum >= 36 && edadNum <= 50) erAmount = 125000;
  else erAmount = 150000;

  const eeAmount = depNum * 25000;

  res.json({
    deductions: [
      { type: "ER", Amount: erAmount }, // Empleador
      { type: "EE", Amount: eeAmount }, // Empleado
    ],
  });
});

module.exports = router;
