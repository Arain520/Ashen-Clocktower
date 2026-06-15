const pptxgen = require("pptxgenjs");
const fs = require("fs");
const path = require("path");

const ROOT = "D:\\UNITY2\\LearnMetroidvania\\Metroidvania";
const WORKSPACE = path.join(ROOT, "outputs", "019eb641-f7e2-7f70-b62a-c91cc2d900d4", "presentations", "ashen-clocktower-course-showcase");
const OUT_DIR = path.join(WORKSPACE, "output");
const OUT_FILE = path.join(OUT_DIR, "灰烬钟塔_完整课程展示PPT.pptx");
fs.mkdirSync(OUT_DIR, { recursive: true });

const ASSETS = {
  player: path.join(ROOT, "Assets", "Animation", "Player", "Player.png"),
  boss: path.join(ROOT, "Assets", "Animation", "Enemy", "BossBringer", "Bringer.png"),
  slime: path.join(ROOT, "Assets", "Animation", "Enemy", "Slime", "Blue Slime Spritesheet.png"),
  bonfire: path.join(ROOT, "Assets", "Animation", "Interact", "CheckPoint", "BonfireBase.png"),
  portal: path.join(ROOT, "Assets", "Animation", "Interact", "Portal", "Portal.png"),
  fire: path.join(ROOT, "Assets", "Animation", "FX", "Particles", "fire.png"),
  dash: path.join(ROOT, "Assets", "Animation", "Interact", "AblityActivators", "DashActivator", "Hermes_Boots.png"),
  bootsFx: path.join(ROOT, "Assets", "Animation", "Interact", "AblityActivators", "DashActivator", "BootsFX.png"),
};

const pptx = new pptxgen();
pptx.layout = "LAYOUT_WIDE";
pptx.author = "Ashen Clocktower Team";
pptx.company = "Course Design";
pptx.subject = "Ashen Clocktower course showcase";
pptx.title = "灰烬钟塔 完整课程展示PPT";
pptx.lang = "zh-CN";
pptx.theme = {
  headFontFace: "Microsoft YaHei",
  bodyFontFace: "Microsoft YaHei",
  lang: "zh-CN",
};
pptx.defineLayout({ name: "WIDE", width: 13.333, height: 7.5 });
pptx.layout = "WIDE";
pptx.margin = 0;

const C = {
  bg: "15110F",
  panel: "211915",
  panel2: "2B211D",
  ink: "F4E6D2",
  muted: "BBA78F",
  dim: "7E6C5A",
  ash: "E08B35",
  fire: "F4B15D",
  red: "B8583E",
  blue: "8DB8D7",
  green: "88B06A",
  line: "4A3427",
  black: "080706",
  white: "FFFFFF",
};

function exists(p) {
  return fs.existsSync(p) && fs.statSync(p).isFile();
}

function addBg(slide, page, title = "") {
  slide.background = { color: C.bg };
  slide.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: 13.333, h: 7.5, fill: { color: C.bg }, line: { color: C.bg } });
  slide.addShape(pptx.ShapeType.rect, { x: 0, y: 0, w: 13.333, h: 0.16, fill: { color: C.ash, transparency: 8 }, line: { color: C.ash, transparency: 100 } });
  slide.addShape(pptx.ShapeType.arc, { x: 10.35, y: -0.85, w: 3.5, h: 3.5, adjustPoint: 0.22, line: { color: C.ash, transparency: 55, pt: 1.2 }, fill: { color: C.bg, transparency: 100 }, rotate: 10 });
  slide.addShape(pptx.ShapeType.arc, { x: 10.82, y: -0.38, w: 2.55, h: 2.55, adjustPoint: 0.32, line: { color: C.fire, transparency: 65, pt: 0.8 }, fill: { color: C.bg, transparency: 100 }, rotate: 110 });
  if (title) {
    slide.addText(title, { x: 0.55, y: 0.36, w: 7.8, h: 0.36, fontFace: "Microsoft YaHei", fontSize: 9.5, color: C.muted, bold: true, charSpace: 1.1 });
  }
  slide.addText(`灰烬钟塔 / Ashen Clocktower · ${page}`, { x: 10.2, y: 7.05, w: 2.55, h: 0.25, fontFace: "Microsoft YaHei", fontSize: 8.5, color: C.dim, align: "right" });
}

function kicker(slide, text, x, y) {
  slide.addShape(pptx.ShapeType.rect, { x, y: y + 0.08, w: 0.42, h: 0.035, fill: { color: C.ash }, line: { color: C.ash, transparency: 100 } });
  slide.addText(text, { x: x + 0.5, y, w: 3.4, h: 0.22, fontFace: "Microsoft YaHei", fontSize: 8.6, color: C.fire, bold: true, charSpace: 1.3, valign: "mid" });
}

function title(slide, text, sub) {
  slide.addText(text, { x: 0.72, y: 0.72, w: 8.4, h: 0.56, fontFace: "Microsoft YaHei", fontSize: 25, bold: true, color: C.ink, breakLine: false, fit: "shrink" });
  if (sub) {
    slide.addText(sub, { x: 0.74, y: 1.28, w: 8.9, h: 0.36, fontFace: "Microsoft YaHei", fontSize: 11.5, color: C.muted, fit: "shrink" });
  }
}

function note(slide, text, x, y, w, h, color = C.panel, line = C.line) {
  slide.addShape(pptx.ShapeType.roundRect, { x, y, w, h, rectRadius: 0.06, fill: { color, transparency: 4 }, line: { color: line, transparency: 10, pt: 0.8 } });
  slide.addText(text, { x: x + 0.17, y: y + 0.14, w: w - 0.34, h: h - 0.24, margin: 0.04, fit: "shrink", fontFace: "Microsoft YaHei", fontSize: 11, color: C.ink, breakLine: false, valign: "mid" });
}

function pill(slide, text, x, y, w, color = C.ash) {
  slide.addShape(pptx.ShapeType.roundRect, { x, y, w, h: 0.42, rectRadius: 0.05, fill: { color, transparency: 5 }, line: { color, transparency: 20, pt: 0.8 } });
  slide.addText(text, { x: x + 0.08, y: y + 0.08, w: w - 0.16, h: 0.22, align: "center", fontFace: "Microsoft YaHei", fontSize: 10.4, color: C.black, bold: true, fit: "shrink" });
}

function bulletList(slide, items, x, y, w, h, opts = {}) {
  const runs = [];
  for (const item of items) {
    runs.push({ text: item, options: { bullet: { type: "bullet" }, breakLine: true } });
  }
  slide.addText(runs, { x, y, w, h, fit: "shrink", fontFace: "Microsoft YaHei", fontSize: opts.size || 11.8, color: opts.color || C.ink, breakLine: false, paraSpaceAfterPt: 7, margin: 0.02 });
}

function addSprite(slide, file, x, y, w, h, transparency = 0) {
  if (exists(file)) {
    slide.addImage({ path: file, x, y, w, h, transparency });
  }
}

function addSection(slide, label, value, x, y, w, h) {
  slide.addText(label, { x, y, w, h: 0.22, fontFace: "Microsoft YaHei", fontSize: 8.5, color: C.fire, bold: true, charSpace: 0.6 });
  slide.addText(value, { x, y: y + 0.25, w, h: h - 0.25, fontFace: "Microsoft YaHei", fontSize: 13, color: C.ink, bold: true, fit: "shrink" });
}

function arrow(slide, x1, y1, x2, y2, color = C.ash) {
  slide.addShape(pptx.ShapeType.line, { x: x1, y: y1, w: x2 - x1, h: y2 - y1, line: { color, pt: 1.4, beginArrowType: "none", endArrowType: "triangle" } });
}

// 1
{
  const slide = pptx.addSlide();
  addBg(slide, 1);
  slide.addShape(pptx.ShapeType.rect, { x: 0, y: 4.85, w: 13.333, h: 2.65, fill: { color: C.black, transparency: 18 }, line: { color: C.black, transparency: 100 } });
  kicker(slide, "COURSE DESIGN SHOWCASE", 0.78, 0.7);
  slide.addText("灰烬钟塔", { x: 0.75, y: 1.2, w: 6.7, h: 0.9, fontFace: "Microsoft YaHei", fontSize: 40, bold: true, color: C.ink });
  slide.addText("Ashen Clocktower", { x: 0.8, y: 2.12, w: 5.2, h: 0.36, fontFace: "Aptos", fontSize: 16, color: C.fire, bold: true });
  slide.addText("战斗夺回灰烬，切相显现过去。", { x: 0.82, y: 2.78, w: 5.9, h: 0.42, fontFace: "Microsoft YaHei", fontSize: 18, color: C.ink, bold: true });
  slide.addText("一款 2D 类银河恶魔城课程设计 Demo：用共享资源把战斗、生存、探索和叙事拧成同一个循环。", { x: 0.84, y: 3.34, w: 6.8, h: 0.58, fontFace: "Microsoft YaHei", fontSize: 12.4, color: C.muted, fit: "shrink" });
  addSprite(slide, ASSETS.player, 8.4, 2.5, 1.2, 1.2);
  addSprite(slide, ASSETS.boss, 9.65, 1.62, 2.3, 2.3, 2);
  addSprite(slide, ASSETS.fire, 8.7, 4.85, 0.7, 0.7);
  note(slide, "展示结构\n项目定位 → 游戏循环 → 简版策划案 → 系统/关卡 → 实现成果 → 展示视频", 0.85, 5.35, 6.15, 0.98, C.panel2);
  note(slide, "Demo 目标\n让评委在 3 分钟内理解“灰烬”如何同时驱动玩法与世界观。", 7.55, 5.35, 4.9, 0.98, "2A1812", C.ash);
}

// 2
{
  const slide = pptx.addSlide();
  addBg(slide, 2, "PROJECT OVERVIEW");
  title(slide, "项目定位：用一个核心资源组织整套体验", "课程展示重点不是堆功能，而是证明“灰烬”能支撑可玩的动作探索循环。");
  const cols = [
    ["类型", "2D 横版动作 / 类银河恶魔城"],
    ["核心卖点", "战斗产出灰烬，切相显现过去地形"],
    ["目标体验", "短程战斗压力 + 地图回路发现 + 叙事残响"],
    ["展示版本", "基础战斗、HUD、灰烬态、叙事触发、房间蓝图"],
  ];
  cols.forEach((c, i) => addSection(slide, c[0], c[1], 0.84 + i * 3.0, 2.0, 2.55, 1.0));
  slide.addShape(pptx.ShapeType.line, { x: 0.8, y: 3.22, w: 11.5, h: 0, line: { color: C.line, pt: 1 } });
  const pillars = [
    ["动作平台跳跃", "移动、攻击、受击反馈构成基础手感。"],
    ["灰烬资源管理", "技能、回血、切相共用同一消耗池。"],
    ["切相探索", "进入灰烬态后旧桥、隐藏结构显现。"],
    ["房间式关卡", "房间独立搭建，传送门统一连接。"],
    ["剧情残响", "短句石碑与旧日长廊把机制嵌入世界观。"],
  ];
  pillars.forEach((p, i) => {
    const x = 0.78 + (i % 3) * 4.05;
    const y = i < 3 ? 3.65 : 5.28;
    note(slide, `${p[0]}\n${p[1]}`, x, y, 3.55, 1.05, i === 1 ? "342218" : C.panel);
  });
}

// 3
{
  const slide = pptx.addSlide();
  addBg(slide, 3, "CORE LOOP");
  title(slide, "核心循环：战斗让玩家获得“触碰过去”的时间", "灰烬不是单独数值，而是战斗、生存、探索三者之间的取舍。");
  const nodes = [
    ["战斗", "攻击敌人\n获得灰烬", 1.0, 2.75, C.red],
    ["灰烬池", "技能 / 回血 / 切相\n共享资源", 3.35, 2.75, C.ash],
    ["切相", "按 R 进入灰烬态\n持续消耗灰烬", 5.7, 2.75, C.fire],
    ["显现旧路", "旧桥 / 跳台 / 隐藏结构\n变成可玩地形", 8.05, 2.75, C.blue],
    ["成长", "投剑 / 冲刺 / 火冰魔法\n打开更深区域", 10.4, 2.75, C.green],
  ];
  nodes.forEach(([t, d, x, y, col]) => {
    slide.addShape(pptx.ShapeType.roundRect, { x, y, w: 1.82, h: 1.1, rectRadius: 0.08, fill: { color: col, transparency: 8 }, line: { color: col, pt: 1 } });
    slide.addText(t, { x: x + 0.14, y: y + 0.16, w: 1.52, h: 0.26, align: "center", fontFace: "Microsoft YaHei", fontSize: 13, color: C.black, bold: true, fit: "shrink" });
    slide.addText(d, { x: x + 0.14, y: y + 0.5, w: 1.52, h: 0.42, align: "center", fontFace: "Microsoft YaHei", fontSize: 8.5, color: C.black, fit: "shrink" });
  });
  for (let i = 0; i < nodes.length - 1; i++) arrow(slide, nodes[i][2] + 1.83, 3.3, nodes[i + 1][2] - 0.1, 3.3);
  arrow(slide, 11.3, 4.05, 1.75, 4.95, C.dim);
  slide.addText("回到早期房间，发现原先不可达路径", { x: 3.15, y: 4.78, w: 5.3, h: 0.28, fontFace: "Microsoft YaHei", fontSize: 11, color: C.muted, align: "center" });
  note(slide, "关键取舍\n玩家可以立刻用灰烬回血/放技能，也可以保留灰烬进入灰烬态通过断桥。资源越紧，探索选择越有重量。", 1.0, 5.45, 5.35, 0.95, "301E17", C.ash);
  note(slide, "展示时一句话\n“打怪不是为了清屏，而是为了把过去临时拉回现在。”", 6.9, 5.45, 4.8, 0.95, C.panel2);
}

// 4
{
  const slide = pptx.addSlide();
  addBg(slide, 4, "GAME DESIGN BRIEF");
  title(slide, "简版策划案：一句话、三支柱、四类玩家动作", "这页用于回答“这个游戏到底怎么设计”的核心问题。");
  note(slide, "一句话概念\n在烧尽的钟塔中，玩家通过战斗收集灰烬，短暂切换到过去状态，让已经消失的平台、桥梁和记忆重新显现。", 0.82, 1.88, 5.6, 1.08, "301E17", C.ash);
  const headers = ["玩法支柱", "系统目标", "关卡目标"];
  const rows = [
    ["动作反馈", "普通攻击、投剑、冲刺与火冰魔法形成层次", "用敌人与地形制造短节奏压力"],
    ["资源取舍", "灰烬同时服务输出、续航和切相", "让“是否现在消耗”成为路线选择"],
    ["过去显现", "灰烬态改变可通行空间与视觉氛围", "断桥、隐藏跳台、旧日长廊承载记忆叙事"],
  ];
  const x0 = 0.82, y0 = 3.35, widths = [2.3, 4.15, 4.55];
  headers.forEach((h, i) => {
    slide.addShape(pptx.ShapeType.rect, { x: x0 + widths.slice(0, i).reduce((a, b) => a + b, 0), y: y0, w: widths[i], h: 0.38, fill: { color: C.ash }, line: { color: C.ash } });
    slide.addText(h, { x: x0 + widths.slice(0, i).reduce((a, b) => a + b, 0) + 0.08, y: y0 + 0.09, w: widths[i] - 0.16, h: 0.18, fontFace: "Microsoft YaHei", fontSize: 9.5, color: C.black, bold: true, align: "center" });
  });
  rows.forEach((r, ri) => {
    const y = y0 + 0.38 + ri * 0.74;
    r.forEach((txt, ci) => {
      const x = x0 + widths.slice(0, ci).reduce((a, b) => a + b, 0);
      slide.addShape(pptx.ShapeType.rect, { x, y, w: widths[ci], h: 0.74, fill: { color: ri % 2 ? C.panel2 : C.panel }, line: { color: C.line, pt: 0.6 } });
      slide.addText(txt, { x: x + 0.12, y: y + 0.13, w: widths[ci] - 0.24, h: 0.46, fontFace: "Microsoft YaHei", fontSize: ci === 0 ? 11 : 9.5, color: ci === 0 ? C.fire : C.ink, bold: ci === 0, fit: "shrink", valign: "mid" });
    });
  });
  addSprite(slide, ASSETS.bonfire, 9.95, 1.7, 0.92, 0.92);
  addSprite(slide, ASSETS.portal, 11.0, 1.62, 0.92, 0.92);
}

// 5
{
  const slide = pptx.addSlide();
  addBg(slide, 5, "PLAYER EXPERIENCE");
  title(slide, "玩家体验路径：从“能打”到“会规划路线”", "教学段要在短时间内完成移动、战斗、灰烬与切相的认知闭环。");
  const steps = [
    ["1", "抵达荒凉小径", "移动 / 跳跃 / 受击反馈"],
    ["2", "遭遇小怪", "攻击、击杀、灰烬值上涨"],
    ["3", "发现断桥", "普通态无法通过，形成目标阻挡"],
    ["4", "石碑提示切相", "按 R 进入灰烬态，旧桥显现"],
    ["5", "获得能力", "投剑 / 冲刺扩展路线"],
    ["6", "回到主塔", "用新能力进入 Boss 前区域"],
  ];
  steps.forEach((s, i) => {
    const x = 0.83 + (i % 3) * 4.03;
    const y = i < 3 ? 2.05 : 4.55;
    slide.addShape(pptx.ShapeType.line, { x: x + 1.82, y: y + 0.54, w: i % 3 === 2 ? 0 : 1.6, h: 0, line: { color: C.line, pt: 1.2, endArrowType: i % 3 === 2 ? "none" : "triangle" } });
    slide.addShape(pptx.ShapeType.ellipse, { x, y, w: 0.72, h: 0.72, fill: { color: C.ash }, line: { color: C.fire, pt: 1 } });
    slide.addText(s[0], { x, y: y + 0.15, w: 0.72, h: 0.26, align: "center", fontFace: "Microsoft YaHei", fontSize: 14, bold: true, color: C.black });
    slide.addText(s[1], { x: x + 0.88, y: y - 0.02, w: 2.4, h: 0.28, fontFace: "Microsoft YaHei", fontSize: 12.2, color: C.ink, bold: true, fit: "shrink" });
    slide.addText(s[2], { x: x + 0.88, y: y + 0.34, w: 2.35, h: 0.44, fontFace: "Microsoft YaHei", fontSize: 9.3, color: C.muted, fit: "shrink" });
  });
  note(slide, "设计原则\n每次给玩家一个明确阻挡，再用灰烬/能力给出解决方式。教学不是弹窗说明，而是“看到做不到 → 获得线索 → 亲手通过”。", 1.25, 6.28, 10.8, 0.62, C.panel2);
}

// 6
{
  const slide = pptx.addSlide();
  addBg(slide, 6, "SYSTEM BREAKDOWN");
  title(slide, "系统拆解：灰烬态让“过去”成为可玩的地形", "四个系统互相扣住：数值、状态、视觉和叙事共同服务同一动作。");
  const systems = [
    ["灰烬系统", "攻击命中与击杀敌人获得灰烬；技能、回血与切相消耗灰烬。", C.ash],
    ["切相系统", "按 R 进入灰烬态；灰烬态下旧桥、跳台与隐藏结构显现。", C.fire],
    ["灰烬态视觉", "画面变暗、橙黄色滤镜、灰烬粒子飘动，帮助玩家区分状态。", C.blue],
    ["叙事触发", "路过石碑或残响点弹出文本，用短句铺设世界观和机制提示。", C.green],
  ];
  systems.forEach((s, i) => {
    const x = 0.86 + (i % 2) * 5.95;
    const y = i < 2 ? 2.02 : 4.08;
    slide.addShape(pptx.ShapeType.roundRect, { x, y, w: 5.28, h: 1.35, rectRadius: 0.07, fill: { color: C.panel }, line: { color: s[2], pt: 1.1, transparency: 10 } });
    slide.addShape(pptx.ShapeType.rect, { x, y, w: 0.12, h: 1.35, fill: { color: s[2] }, line: { color: s[2] } });
    slide.addText(s[0], { x: x + 0.32, y: y + 0.22, w: 2.2, h: 0.28, fontFace: "Microsoft YaHei", fontSize: 14, color: C.fire, bold: true });
    slide.addText(s[1], { x: x + 0.32, y: y + 0.63, w: 4.55, h: 0.44, fontFace: "Microsoft YaHei", fontSize: 10.2, color: C.ink, fit: "shrink" });
  });
  addSprite(slide, ASSETS.fire, 5.75, 5.94, 0.42, 0.42);
  slide.addText("钟、灰烬、残响、过去结构：所有视觉符号都回到“时间未熄灭”的主题。", { x: 0.95, y: 6.08, w: 10.6, h: 0.28, fontFace: "Microsoft YaHei", fontSize: 11.4, color: C.muted, align: "center" });
}

// 7
{
  const slide = pptx.addSlide();
  addBg(slide, 7, "LEVEL DESIGN");
  title(slide, "关卡蓝图：房间式结构 + 传送门连接", "地图按可教学、可回环、可扩展三类需求组织。");
  const rooms = [
    ["荒凉小径", 0.92, 2.85, C.dim],
    ["断桥教学", 2.38, 2.85, C.ash],
    ["钟塔前庭", 3.94, 2.85, C.dim],
    ["钟塔大厅 Hub", 5.55, 2.85, C.fire],
    ["齿轮廊道\n冲刺", 7.36, 1.75, C.blue],
    ["熔灰工坊\n火魔法", 7.36, 3.95, C.red],
    ["静霜钟室\n冰魔法", 9.0, 1.75, C.blue],
    ["旧日长廊\n赐福", 9.0, 3.95, C.green],
    ["主塔登顶\nBoss", 10.82, 2.85, C.ash],
  ];
  rooms.forEach(([r, x, y, col]) => {
    slide.addShape(pptx.ShapeType.roundRect, { x, y, w: 1.28, h: 0.72, rectRadius: 0.06, fill: { color: col, transparency: 12 }, line: { color: col, pt: 1 } });
    slide.addText(r, { x: x + 0.08, y: y + 0.12, w: 1.12, h: 0.4, fontFace: "Microsoft YaHei", fontSize: 8.7, color: C.black, bold: true, align: "center", fit: "shrink" });
  });
  const lines = [[2.2,3.21,2.38,3.21],[3.66,3.21,3.94,3.21],[5.22,3.21,5.55,3.21],[6.83,3.05,7.36,2.11],[6.83,3.35,7.36,4.31],[8.64,2.11,9.0,2.11],[8.64,4.31,9.0,4.31],[10.28,2.11,10.82,3.05],[10.28,4.31,10.82,3.35]];
  lines.forEach(l => arrow(slide, ...l, C.line));
  note(slide, "蓝图工具目标\n每个房间独立绘制，出入口统一用传送门连接；后续可以把教学房、能力房、Boss 房拆成可复用模块。", 1.0, 5.75, 5.4, 0.78, C.panel2);
  note(slide, "课程展示重点\n展示“断桥切相”这条最短体验闭环，再用地图蓝图说明它可以扩展成完整类银河恶魔城结构。", 6.85, 5.75, 5.15, 0.78, "2E1D16", C.ash);
}

// 8
{
  const slide = pptx.addSlide();
  addBg(slide, 8, "CONTENT PLAN");
  title(slide, "内容推进：剧情、能力、关卡在同一条登塔线上收束", "不是单独讲故事，而是让每段剧情都解释一种新的可玩规则。");
  const stages = [
    ["开场", "远行者抵达钟塔，进入荒野小径"],
    ["教学", "击杀小怪、篝火休整、首次获得灰烬"],
    ["灰烬态", "断桥受阻，石碑提示切相，旧桥显现"],
    ["入塔", "获得投掷宝剑，打开钟塔入口"],
    ["成长", "冲刺 / 火 / 冰逐步解锁，理解钟塔真相"],
    ["赐福", "旧日长廊回应，灰烬态不再消耗"],
    ["终局", "攀上塔顶，与守护者 Boss 战斗"],
  ];
  stages.forEach((s, i) => {
    const x = 0.76 + i * 1.72;
    const y = 2.48 + (i % 2) * 0.85;
    slide.addShape(pptx.ShapeType.ellipse, { x, y, w: 0.42, h: 0.42, fill: { color: i === 6 ? C.ash : C.panel2 }, line: { color: C.ash, pt: 1 } });
    if (i < stages.length - 1) slide.addShape(pptx.ShapeType.line, { x: x + 0.43, y: y + 0.21, w: 1.28, h: ((i + 1) % 2) * 0.85 - (i % 2) * 0.85, line: { color: C.line, pt: 1.2, endArrowType: "triangle" } });
    slide.addText(s[0], { x: x - 0.1, y: y + 0.55, w: 1.2, h: 0.24, fontFace: "Microsoft YaHei", fontSize: 10.5, color: C.fire, bold: true, align: "center" });
    slide.addText(s[1], { x: x - 0.22, y: y + 0.86, w: 1.35, h: 0.55, fontFace: "Microsoft YaHei", fontSize: 7.8, color: C.muted, align: "center", fit: "shrink" });
  });
  note(slide, "旧日长廊\n承载好友昵称、头像、寄语与 BGM，作为“记忆的灰烬”；它也是灰烬态机制的情感解释。", 1.02, 5.55, 5.55, 0.88, "301E17", C.ash);
  note(slide, "Boss 前收束\n前面学习的移动、灰烬取舍、切相观察、能力使用都会在塔顶战斗前得到回收。", 6.98, 5.55, 4.95, 0.88, C.panel2);
}

// 9
{
  const slide = pptx.addSlide();
  addBg(slide, 9, "IMPLEMENTATION");
  title(slide, "实现成果：已能支撑一次完整课堂演示", "把功能按“可演示、正在推进、下一步”三层讲清楚，评委更容易判断完成度。");
  const blocks = [
    ["已完成 / 已有雏形", ["灰烬系统与灰烬值 UI", "切相系统与灰烬态物体", "基础战斗、敌人、Boss 资源", "中文菜单、对话/石碑触发", "宣传海报与房间蓝图工具"], C.green],
    ["正在推进", ["精细化房间蓝图", "单向平台与隐藏路径", "旧日长廊内容征集", "能力获取提示文案", "关键资产制作与替换"], C.ash],
    ["下一步重点", ["打通 Demo 主路径", "荒凉小径 → 断桥切相", "塔前战斗 → 投剑入塔", "主塔登顶预览", "录制 60-90 秒展示视频"], C.fire],
  ];
  blocks.forEach((b, i) => {
    const x = 0.84 + i * 4.05;
    slide.addShape(pptx.ShapeType.roundRect, { x, y: 2.08, w: 3.55, h: 3.78, rectRadius: 0.07, fill: { color: C.panel }, line: { color: b[2], pt: 1 } });
    slide.addText(b[0], { x: x + 0.2, y: 2.32, w: 3.12, h: 0.3, fontFace: "Microsoft YaHei", fontSize: 13, bold: true, color: C.fire, align: "center", fit: "shrink" });
    bulletList(slide, b[1], x + 0.35, 2.92, 2.85, 2.35, { size: 9.8 });
  });
  addSprite(slide, ASSETS.dash, 1.13, 6.08, 0.38, 0.38);
  addSprite(slide, ASSETS.bootsFx, 1.6, 6.08, 0.38, 0.38);
  slide.addText("课程展示目标：让评委在 3 分钟内理解“灰烬”如何同时驱动战斗、探索、叙事与关卡设计。", { x: 2.1, y: 6.15, w: 8.95, h: 0.28, fontFace: "Microsoft YaHei", fontSize: 11.3, color: C.muted, align: "center", fit: "shrink" });
}

// 10
{
  const slide = pptx.addSlide();
  addBg(slide, 10, "DEMO SCRIPT");
  title(slide, "课堂演示脚本：90 秒跑完完整体验闭环", "这页可以作为正式演讲时的后台提词，也能指导录屏素材怎么剪。");
  const shots = [
    ["00-15s", "主菜单与开场", "展示标题、开场文字、玩家落地与基础移动"],
    ["15-35s", "战斗与灰烬", "击杀小怪，灰烬值 UI 上涨，篝火点短暂停留"],
    ["35-55s", "断桥与切相", "普通态无法通过，按 R 进入灰烬态，旧桥显现"],
    ["55-75s", "能力与路线", "投掷宝剑 / 冲刺 / 火冰魔法展示路线扩展"],
    ["75-90s", "登塔与 Boss", "进入主塔或 Boss 区域，收束“登上钟塔”的目标"],
  ];
  shots.forEach((s, i) => {
    const y = 1.9 + i * 0.82;
    slide.addShape(pptx.ShapeType.rect, { x: 0.92, y, w: 1.15, h: 0.5, fill: { color: i === 2 ? C.ash : C.panel2 }, line: { color: C.line, pt: 0.6 } });
    slide.addText(s[0], { x: 1.0, y: y + 0.15, w: 0.98, h: 0.16, fontFace: "Aptos", fontSize: 9, bold: true, color: i === 2 ? C.black : C.fire, align: "center" });
    slide.addText(s[1], { x: 2.34, y: y + 0.05, w: 2.0, h: 0.22, fontFace: "Microsoft YaHei", fontSize: 12, bold: true, color: C.ink });
    slide.addText(s[2], { x: 4.45, y: y + 0.07, w: 6.2, h: 0.22, fontFace: "Microsoft YaHei", fontSize: 10.2, color: C.muted, fit: "shrink" });
    if (i < shots.length - 1) slide.addShape(pptx.ShapeType.line, { x: 1.5, y: y + 0.5, w: 0, h: 0.32, line: { color: C.line, pt: 1, endArrowType: "triangle" } });
  });
  note(slide, "录制建议\n画面优先展示 UI 变化和地形变化；每个镜头保持 2 秒以上可读时间，避免快速切菜单。", 0.94, 6.22, 5.3, 0.58, C.panel2);
  note(slide, "讲解一句话\n“玩家用战斗赚取灰烬，再用灰烬把过去临时变成现在的道路。”", 6.78, 6.22, 4.9, 0.58, "301E17", C.ash);
}

// 11
{
  const slide = pptx.addSlide();
  addBg(slide, 11, "RISK & PLAN");
  title(slide, "风险与后续计划：优先打磨最短可展示链路", "课程项目的范围控制：先完整，再丰富。");
  const roadmap = [
    ["本周", "打通主路径", "小径 → 断桥 → 塔前 → 入塔"],
    ["下阶段", "补齐反馈", "受击、击杀、切相、拾取音画反馈"],
    ["展示前", "录制视频", "压缩成 60-90 秒，嵌入最后一页"],
  ];
  roadmap.forEach((r, i) => {
    const x = 1.0 + i * 3.75;
    slide.addShape(pptx.ShapeType.chevron, { x, y: 2.22, w: 3.1, h: 1.02, fill: { color: i === 0 ? C.ash : C.panel2 }, line: { color: i === 0 ? C.ash : C.line, pt: 1 } });
    slide.addText(r[0], { x: x + 0.24, y: 2.42, w: 0.9, h: 0.22, fontFace: "Microsoft YaHei", fontSize: 11, color: i === 0 ? C.black : C.fire, bold: true });
    slide.addText(r[1], { x: x + 1.06, y: 2.32, w: 1.7, h: 0.25, fontFace: "Microsoft YaHei", fontSize: 12.5, color: i === 0 ? C.black : C.ink, bold: true, fit: "shrink" });
    slide.addText(r[2], { x: x + 1.06, y: 2.67, w: 1.55, h: 0.22, fontFace: "Microsoft YaHei", fontSize: 8.4, color: i === 0 ? C.black : C.muted, fit: "shrink" });
  });
  const risks = [
    ["范围风险", "地图过大导致主路径不完整。对策：优先保留 1 条必经路线。"],
    ["可读性风险", "切相地形不够明显。对策：灰烬态滤镜、粒子、轮廓统一强化。"],
    ["演示风险", "现场操作失误。对策：PPT 末页附录屏，现场 Demo 作为加分项。"],
  ];
  risks.forEach((r, i) => note(slide, `${r[0]}\n${r[1]}`, 1.02 + i * 3.78, 4.38, 3.2, 1.05, i === 2 ? "301E17" : C.panel, i === 2 ? C.ash : C.line));
}

// 12
{
  const slide = pptx.addSlide();
  addBg(slide, 12, "DEMO VIDEO");
  title(slide, "展示视频：完整体验闭环", "当前目录未发现 MP4；此页已预留正式视频位，并附 Demo 程序路径。");
  slide.addShape(pptx.ShapeType.roundRect, { x: 0.92, y: 1.75, w: 7.4, h: 4.55, rectRadius: 0.08, fill: { color: C.black, transparency: 3 }, line: { color: C.ash, pt: 1.3 } });
  slide.addShape(pptx.ShapeType.triangle, { x: 4.02, y: 3.26, w: 0.95, h: 0.95, rotate: 90, fill: { color: C.ash }, line: { color: C.ash } });
  slide.addText("将 60-90 秒录屏 MP4 拖入此区域", { x: 2.22, y: 4.55, w: 4.85, h: 0.28, fontFace: "Microsoft YaHei", fontSize: 13.5, color: C.ink, bold: true, align: "center" });
  slide.addText("推荐内容：战斗获得灰烬 → 按 R 切相 → 旧桥显现 → 获得能力 → 登塔 / Boss", { x: 1.52, y: 4.95, w: 6.2, h: 0.24, fontFace: "Microsoft YaHei", fontSize: 9.8, color: C.muted, align: "center", fit: "shrink" });
  note(slide, "展示包路径\nD:\\UNITY2\\AshenClocktower\\Metroidvania.exe", 8.75, 2.0, 3.55, 0.9, "301E17", C.ash);
  slide.addText("点击运行 Demo", {
    x: 9.06, y: 3.15, w: 2.92, h: 0.45,
    fontFace: "Microsoft YaHei", fontSize: 15, bold: true, color: C.black, align: "center", valign: "mid",
    fill: { color: C.ash }, line: { color: C.fire, pt: 1 },
    hyperlink: { url: "file:///D:/UNITY2/AshenClocktower/Metroidvania.exe", tooltip: "运行本地 Demo 包" },
  });
  bulletList(slide, [
    "正式提交前将录屏文件命名为 AshenClocktower_Demo.mp4",
    "PowerPoint 中使用“插入 → 视频 → 此设备”替换左侧占位",
    "保留右侧 Demo 路径，便于现场补充运行",
  ], 8.86, 4.12, 3.25, 1.1, { size: 9.6, color: C.ink });
  slide.addText("灰烬不是死亡后的残余，而是时间不愿忘记的东西。", { x: 0.98, y: 6.62, w: 7.1, h: 0.28, fontFace: "Microsoft YaHei", fontSize: 12.2, color: C.fire, bold: true });
}

pptx.writeFile({ fileName: OUT_FILE });
